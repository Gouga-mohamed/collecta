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

    public GetInvoiceByIdQueryHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<InvoiceDetailDto?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (invoice == null) return null;

        var dto = GetInvoicesQueryHandler.MapToDto(invoice, invoice.Customer.Name);
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
            Payments = invoice.Payments.Select(p => GetPaymentsQueryHandler.MapToDto(p, invoice.Customer.Name, invoice.InvoiceNumber)).ToList()
        };
    }
}
