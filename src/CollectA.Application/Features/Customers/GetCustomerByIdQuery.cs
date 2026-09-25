using CollectA.Application.Features.Invoices;
using CollectA.Application.Features.Payments;
using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Domain.Common.Interfaces;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Customers;

public class GetCustomerByIdQuery : IRequest<CustomerDetailDto?>
{
    public Guid Id { get; set; }
}

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public GetCustomerByIdQueryHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<CustomerDetailDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(c => c.Invoices)
            .Include(c => c.Payments)
            .Include(c => c.CollectionActions).ThenInclude(a => a.Invoice)
            .Include(c => c.Promises).ThenInclude(p => p.Invoice)
            .Include(c => c.Disputes).ThenInclude(d => d.Invoice)
            .Include(c => c.Contacts)
            .Include(c => c.RiskScores.OrderByDescending(r => r.CalculatedAt).Take(1))
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null) return null;

        var dto = GetCustomersQueryHandler.MapToDto(customer);
        var detail = new CustomerDetailDto
        {
            Id = dto.Id,
            Code = dto.Code,
            Name = dto.Name,
            LegalName = dto.LegalName,
            TaxId = dto.TaxId,
            Industry = dto.Industry,
            City = dto.City,
            Country = dto.Country,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            PaymentTermsDays = dto.PaymentTermsDays,
            CreditLimit = dto.CreditLimit,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            TotalInvoiced = dto.TotalInvoiced,
            TotalPaid = dto.TotalPaid,
            TotalDue = dto.TotalDue,
            TotalOverdue = dto.TotalOverdue,
            RiskLevel = dto.RiskLevel,
            TradeRegister = customer.TradeRegister,
            Address = customer.Address,
            Website = customer.Website,
            Notes = customer.Notes,
            Contacts = customer.Contacts.Select(x => new CustomerContactDto
            {
                Id = x.Id,
                Name = $"{x.FirstName} {x.LastName}".Trim(),
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                JobTitle = x.JobTitle,
                IsPrimary = x.IsPrimary
            }).ToList(),
            RecentInvoices = customer.Invoices.OrderByDescending(i => i.InvoiceDate).Take(5)
                .Select(i => GetInvoicesQueryHandler.MapToDto(i, customer.Name)).ToList(),
            RecentPayments = customer.Payments.OrderByDescending(p => p.PaymentDate).Take(5)
                .Select(p => GetPaymentsQueryHandler.MapToDto(p, customer.Name, null)).ToList(),
            RecentActions = await MapActionsAsync(customer.CollectionActions.OrderByDescending(a => a.ActionDate).Take(5), cancellationToken),
            RecentPromises = await MapPromisesAsync(customer.Promises.OrderByDescending(p => p.PromiseDate).Take(5), cancellationToken),
            RecentDisputes = await MapDisputesAsync(customer.Disputes.OrderByDescending(d => d.CreatedAt).Take(5), cancellationToken)
        };

        return detail;
    }

    private async Task<List<CollectionActionDto>> MapActionsAsync(IEnumerable<Domain.Entities.CollectionAction> actions, CancellationToken cancellationToken)
    {
        var dtos = new List<CollectionActionDto>();
        foreach (var action in actions)
        {
            var assignedToName = action.AssignedToId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(action.AssignedToId.Value, cancellationToken)
                : null;
            var createdByName = await _userLookupService.GetUserFullNameAsync(action.CreatedById, cancellationToken);
            dtos.Add(CollectionActions.GetCollectionActionsQueryHandler.MapToDto(action, action.Customer?.Name ?? string.Empty, action.Invoice?.InvoiceNumber, assignedToName, createdByName));
        }
        return dtos;
    }

    private async Task<List<PromiseToPayDto>> MapPromisesAsync(IEnumerable<Domain.Entities.PromiseToPay> promises, CancellationToken cancellationToken)
    {
        var dtos = new List<PromiseToPayDto>();
        foreach (var promise in promises)
        {
            var responsibleName = promise.ResponsibleAgentId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(promise.ResponsibleAgentId.Value, cancellationToken)
                : null;
            dtos.Add(Promises.GetPromisesQueryHandler.MapToDto(promise, promise.Customer?.Name ?? string.Empty, promise.Invoice?.InvoiceNumber, responsibleName));
        }
        return dtos;
    }

    private async Task<List<DisputeDto>> MapDisputesAsync(IEnumerable<Domain.Entities.Dispute> disputes, CancellationToken cancellationToken)
    {
        var dtos = new List<DisputeDto>();
        foreach (var dispute in disputes)
        {
            var responsibleName = dispute.ResponsibleId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(dispute.ResponsibleId.Value, cancellationToken)
                : null;
            dtos.Add(Disputes.GetDisputesQueryHandler.MapToDto(dispute, dispute.Customer?.Name ?? string.Empty, dispute.Invoice?.InvoiceNumber, responsibleName));
        }
        return dtos;
    }
}
