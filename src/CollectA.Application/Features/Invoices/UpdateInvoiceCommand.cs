using CollectA.Application.Common.Dtos;
using CollectA.Domain.Enums;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Invoices;

public class UpdateInvoiceCommand : IRequest<InvoiceDto>
{
    public Guid Id { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "DZD";
    public string? Description { get; set; }
    public int PaymentTermsDays { get; set; }
    public InvoiceStatus Status { get; set; }
    public bool IsDisputed { get; set; }
}

public class UpdateInvoiceCommandHandler : IRequestHandler<UpdateInvoiceCommand, InvoiceDto>
{
    private readonly IIIApplicationDbContext _context;

    public UpdateInvoiceCommandHandler(IIIApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceDto> Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (invoice == null) throw new KeyNotFoundException($"Invoice '{request.Id}' not found.");

        invoice.InvoiceDate = request.InvoiceDate;
        invoice.DueDate = request.DueDate;
        invoice.Amount = request.Amount;
        invoice.Currency = request.Currency;
        invoice.Description = request.Description;
        invoice.PaymentTermsDays = request.PaymentTermsDays;
        invoice.Status = request.Status;
        invoice.IsDisputed = request.IsDisputed;

        RecalculateInvoiceStatus(invoice);

        await _context.SaveChangesAsync(cancellationToken);

        return GetInvoicesQuery.MapToDto(invoice, invoice.Customer.Name);
    }

    public static void RecalculateInvoiceStatus(Domain.Entities.Invoice invoice)
    {
        if (invoice.Status == InvoiceStatus.Cancelled || invoice.Status == InvoiceStatus.WrittenOff) return;

        if (invoice.PaidAmount >= invoice.Amount)
        {
            invoice.Status = InvoiceStatus.Paid;
        }
        else if (invoice.PaidAmount > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }
        else if (invoice.DueDate < DateTime.UtcNow.Date && invoice.Amount > 0)
        {
            invoice.Status = InvoiceStatus.Overdue;
        }
        else if (invoice.IsDisputed)
        {
            invoice.Status = InvoiceStatus.Disputed;
        }
        else
        {
            invoice.Status = InvoiceStatus.Open;
        }
    }
}
