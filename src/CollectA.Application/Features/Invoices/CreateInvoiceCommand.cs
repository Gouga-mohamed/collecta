using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Entities;
using CollectA.Domain.Enums;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Invoices;

public class CreateInvoiceCommand : IRequest<InvoiceDto>
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "DZD";
    public string? Description { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    public List<CreateInvoiceLineCommand> Lines { get; set; } = new();
}

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, InvoiceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateInvoiceCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<InvoiceDto> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.ForTenant(_tenantContext).FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);
        if (customer == null) throw new KeyNotFoundException($"Customer '{request.CustomerId}' not found.");

        if (await _context.Invoices.ForTenant(_tenantContext).AnyAsync(i => i.InvoiceNumber == request.InvoiceNumber, cancellationToken))
        {
            throw new InvalidOperationException($"Invoice number '{request.InvoiceNumber}' already exists.");
        }

        var lines = request.Lines.Any()
            ? request.Lines.Select(l => new InvoiceLine
            {
                Id = Guid.NewGuid(),
                Description = l.Description,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
            : new List<InvoiceLine>();

        var amount = lines.Any() ? lines.Sum(l => l.Amount) : request.Amount;

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = request.InvoiceNumber,
            CustomerId = request.CustomerId,
            InvoiceDate = request.InvoiceDate,
            DueDate = request.DueDate,
            Amount = amount,
            PaidAmount = 0,
            Currency = request.Currency,
            Status = amount > 0 ? InvoiceStatus.Open : InvoiceStatus.Draft,
            Description = request.Description,
            PaymentTermsDays = request.PaymentTermsDays,
            Lines = lines
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        return GetInvoicesQueryHandler.MapToDto(invoice, customer.Name);
    }
}
