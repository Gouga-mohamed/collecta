using CollectA.Application.Features.Invoices;
using CollectA.Application.Features.Payments;
using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Domain.Common.Interfaces;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Invoices;

public class GetInvoiceByIdQuery : IRequest<InvoiceDetailDto?>
{
    public Guid Id { get; set; }
}

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public GetInvoiceByIdQueryHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<InvoiceDetailDto?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .Include(i => i.CollectionActions)
            .Include(i => i.Promises)
            .Include(i => i.Disputes)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (invoice == null) return null;

        var dto = GetInvoicesQueryHandler.MapToDto(invoice, invoice.Customer.Name);

        var actions = new List<CollectionActionDto>();
        foreach (var action in invoice.CollectionActions.OrderByDescending(a => a.ActionDate).Take(10))
        {
            var assignedToName = action.AssignedToId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(action.AssignedToId.Value, cancellationToken)
                : null;
            var createdByName = await _userLookupService.GetUserFullNameAsync(action.CreatedById, cancellationToken);
            actions.Add(CollectionActions.GetCollectionActionsQueryHandler.MapToDto(action, invoice.Customer.Name, invoice.InvoiceNumber, assignedToName, createdByName));
        }

        var promises = new List<PromiseToPayDto>();
        foreach (var promise in invoice.Promises.OrderByDescending(p => p.PromiseDate).Take(10))
        {
            var responsibleName = promise.ResponsibleAgentId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(promise.ResponsibleAgentId.Value, cancellationToken)
                : null;
            promises.Add(Promises.GetPromisesQueryHandler.MapToDto(promise, invoice.Customer.Name, invoice.InvoiceNumber, responsibleName));
        }

        var disputes = new List<DisputeDto>();
        foreach (var dispute in invoice.Disputes.OrderByDescending(d => d.CreatedAt).Take(10))
        {
            var responsibleName = dispute.ResponsibleId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(dispute.ResponsibleId.Value, cancellationToken)
                : null;
            disputes.Add(Disputes.GetDisputesQueryHandler.MapToDto(dispute, invoice.Customer.Name, invoice.InvoiceNumber, responsibleName));
        }

        return new InvoiceDetailDto
        {
            Id = dto.Id,
            InvoiceNumber = dto.InvoiceNumber,
            CustomerId = dto.CustomerId,
            CustomerName = dto.CustomerName,
            InvoiceDate = dto.InvoiceDate,
            DueDate = dto.DueDate,
            Amount = dto.Amount,
            PaidAmount = dto.PaidAmount,
            Currency = dto.Currency,
            Status = dto.Status,
            Description = dto.Description,
            PaymentTermsDays = dto.PaymentTermsDays,
            IsDisputed = dto.IsDisputed,
            DaysOverdue = dto.DaysOverdue,
            IsOverdue = dto.IsOverdue,
            CreatedAt = dto.CreatedAt,
            Lines = invoice.Lines.Select(l => new InvoiceLineDto
            {
                Id = l.Id,
                Description = l.Description,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                Amount = l.Amount
            }).ToList(),
            Payments = invoice.Payments.Select(p => GetPaymentsQueryHandler.MapToDto(p, invoice.Customer.Name, invoice.InvoiceNumber)).ToList(),
            Actions = actions,
            Promises = promises,
            Disputes = disputes
        };
    }
}
