using CollectA.Application.Features.Invoices;
using CollectA.Application.Features.Payments;
using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Domain.Common.Interfaces;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskStatus = CollectA.Domain.Enums.TaskStatus;

namespace CollectA.Application.Features.Customers;

public class GetCustomer360Query : IRequest<Customer360Dto?>
{
    public Guid Id { get; set; }
}

public class GetCustomer360QueryHandler : IRequestHandler<GetCustomer360Query, Customer360Dto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public GetCustomer360QueryHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<Customer360Dto?> Handle(GetCustomer360Query request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(c => c.Invoices)
            .Include(c => c.Payments)
            .Include(c => c.Contacts)
            .Include(c => c.RiskScores.OrderByDescending(r => r.CalculatedAt).Take(1))
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null) return null;

        var detail = await new GetCustomerByIdQueryHandler(_context, _tenantContext, _userLookupService)
            .Handle(new GetCustomerByIdQuery { Id = request.Id }, cancellationToken);

        var totalInvoiced = customer.Invoices.Sum(i => i.Amount);
        var totalPaid = customer.Invoices.Sum(i => i.PaidAmount);
        var averageDailySales = totalInvoiced / 365m;
        var dso = averageDailySales > 0 ? (int)(customer.Invoices.Sum(i => i.RemainingAmount) / averageDailySales) : 0;

        var openInvoices = customer.Invoices.Where(i => i.Status != Domain.Enums.InvoiceStatus.Paid && i.Status != Domain.Enums.InvoiceStatus.Cancelled && i.Status != Domain.Enums.InvoiceStatus.WrittenOff);

        var recentActions = await GetRecentActionsAsync(request.Id, cancellationToken);
        var openTasks = await GetOpenTasksAsync(request.Id, cancellationToken);
        var activePromises = await GetActivePromisesAsync(request.Id, cancellationToken);
        var openDisputes = await GetOpenDisputesAsync(request.Id, cancellationToken);

        return new Customer360Dto
        {
            Customer = detail!,
            OpenInvoicesCount = openInvoices.Count(),
            OverdueInvoicesCount = openInvoices.Count(i => i.IsOverdue),
            DsoApproximate = dso,
            Invoices = customer.Invoices.OrderByDescending(i => i.InvoiceDate)
                .Select(i => GetInvoicesQueryHandler.MapToDto(i, customer.Name)).ToList(),
            Payments = customer.Payments.OrderByDescending(p => p.PaymentDate)
                .Select(p => GetPaymentsQueryHandler.MapToDto(p, customer.Name, null)).ToList(),
            Notes = new List<NoteDto>(),
            RecentActions = recentActions,
            OpenTasks = openTasks,
            ActivePromises = activePromises,
            OpenDisputes = openDisputes
        };
    }

    private async Task<List<CollectionActionDto>> GetRecentActionsAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var actions = await _context.CollectionActions
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(a => a.Customer)
            .Include(a => a.Invoice)
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.ActionDate)
            .Take(5)
            .ToListAsync(cancellationToken);

        var dtos = new List<CollectionActionDto>();
        foreach (var action in actions)
        {
            var assignedToName = action.AssignedToId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(action.AssignedToId.Value, cancellationToken)
                : null;
            var createdByName = await _userLookupService.GetUserFullNameAsync(action.CreatedById, cancellationToken);
            dtos.Add(CollectionActions.GetCollectionActionsQueryHandler.MapToDto(action, action.Customer.Name, action.Invoice?.InvoiceNumber, assignedToName, createdByName));
        }

        return dtos;
    }

    private async Task<List<CollectionTaskDto>> GetOpenTasksAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var tasks = await _context.CollectionTasks
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(t => t.Customer)
            .Include(t => t.Invoice)
            .Where(t => t.CustomerId == customerId && t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled)
            .OrderBy(t => t.DueDate)
            .Take(10)
            .ToListAsync(cancellationToken);

        var dtos = new List<CollectionTaskDto>();
        foreach (var task in tasks)
        {
            var assignedToName = task.AssignedToId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(task.AssignedToId.Value, cancellationToken)
                : null;
            dtos.Add(CollectionTasks.GetCollectionTasksQueryHandler.MapToDto(task, task.Customer.Name, task.Invoice?.InvoiceNumber, assignedToName));
        }

        return dtos;
    }

    private async Task<List<PromiseToPayDto>> GetActivePromisesAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var promises = await _context.PromiseToPays
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(p => p.Customer)
            .Include(p => p.Invoice)
            .Where(p => p.CustomerId == customerId && (p.Status == PromiseStatus.Pending || p.Status == PromiseStatus.PartiallyFulfilled))
            .OrderBy(p => p.PromiseDate)
            .Take(10)
            .ToListAsync(cancellationToken);

        var dtos = new List<PromiseToPayDto>();
        foreach (var promise in promises)
        {
            var responsibleName = promise.ResponsibleAgentId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(promise.ResponsibleAgentId.Value, cancellationToken)
                : null;
            dtos.Add(Promises.GetPromisesQueryHandler.MapToDto(promise, promise.Customer.Name, promise.Invoice?.InvoiceNumber, responsibleName));
        }

        return dtos;
    }

    private async Task<List<DisputeDto>> GetOpenDisputesAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var disputes = await _context.Disputes
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(d => d.Customer)
            .Include(d => d.Invoice)
            .Where(d => d.CustomerId == customerId && (d.Status == DisputeStatus.Open || d.Status == DisputeStatus.Investigating || d.Status == DisputeStatus.WaitingCustomer || d.Status == DisputeStatus.WaitingInternal))
            .OrderByDescending(d => d.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        var dtos = new List<DisputeDto>();
        foreach (var dispute in disputes)
        {
            var responsibleName = dispute.ResponsibleId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(dispute.ResponsibleId.Value, cancellationToken)
                : null;
            dtos.Add(Disputes.GetDisputesQueryHandler.MapToDto(dispute, dispute.Customer.Name, dispute.Invoice?.InvoiceNumber, responsibleName));
        }

        return dtos;
    }
}
