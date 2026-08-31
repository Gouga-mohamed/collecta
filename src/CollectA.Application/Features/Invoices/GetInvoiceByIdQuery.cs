using CollectA.Application.Common.Dtos;
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
    private readonly IIIApplicationDbContext _context;

    public GetInvoiceByIdQueryHandler(IIIApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceDetailDto?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .Include(i => i.CollectionActions)
            .Include(i => i.Promises)
            .Include(i => i.Disputes)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (invoice == null) return null;

        var dto = GetInvoicesQuery.MapToDto(invoice, invoice.Customer.Name);
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
            Payments = invoice.Payments.Select(p => GetPaymentsQuery.MapToDto(p, invoice.Customer.Name, invoice.InvoiceNumber)).ToList(),
            CollectionActions = invoice.CollectionActions.Select(a => GetCollectionActionsQuery.MapToDto(a, invoice.Customer.Name)).ToList(),
            Promises = invoice.Promises.Select(p => GetPromisesQuery.MapToDto(p, invoice.Customer.Name)).ToList(),
            Disputes = invoice.Disputes.Select(d => GetDisputesQuery.MapToDto(d, invoice.Customer.Name)).ToList()
        };
    }
}
