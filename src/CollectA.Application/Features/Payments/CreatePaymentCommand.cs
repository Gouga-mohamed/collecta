using CollectA.Application.Common.Dtos;
using CollectA.Domain.Entities;
using CollectA.Domain.Enums;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Payments;

public class CreatePaymentCommand : IRequest<PaymentDto>
{
    public Guid CustomerId { get; set; }
    public Guid? InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "DZD";
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;
    public string? Reference { get; set; }
    public string? BankName { get; set; }
    public string? Notes { get; set; }
    public CreateChequeCommand? Cheque { get; set; }
}

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentDto>
{
    private readonly IIIApplicationDbContext _context;

    public CreatePaymentCommandHandler(IIIApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);
        if (customer == null) throw new KeyNotFoundException($"Customer '{request.CustomerId}' not found.");

        Domain.Entities.Invoice? invoice = null;
        if (request.InvoiceId.HasValue)
        {
            invoice = await _context.Invoices
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.Id == request.InvoiceId.Value && i.CustomerId == request.CustomerId, cancellationToken);
            if (invoice == null) throw new KeyNotFoundException($"Invoice '{request.InvoiceId}' not found for this customer.");
        }

        Cheque? cheque = null;
        if (request.Method == PaymentMethod.Cheque && request.Cheque != null)
        {
            cheque = new Cheque
            {
                Id = Guid.NewGuid(),
                ChequeNumber = request.Cheque.ChequeNumber,
                BankName = request.Cheque.BankName,
                Drawer = request.Cheque.Drawer,
                DueDate = request.Cheque.DueDate,
                Amount = request.Amount,
                Currency = request.Currency,
                Notes = request.Cheque.Notes,
                Status = ChequeStatus.Received
            };
            _context.Cheques.Add(cheque);
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            InvoiceId = request.InvoiceId,
            Amount = request.Amount,
            Currency = request.Currency,
            PaymentDate = request.PaymentDate,
            Method = request.Method,
            Reference = request.Reference,
            BankName = request.BankName,
            Notes = request.Notes,
            Cheque = cheque,
            ChequeId = cheque?.Id
        };

        _context.Payments.Add(payment);

        if (invoice != null)
        {
            invoice.PaidAmount += request.Amount;
            UpdateInvoiceStatus.Recalculate(invoice);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return GetPaymentsQuery.MapToDto(payment, customer.Name, invoice?.InvoiceNumber);
    }
}

public static class UpdateInvoiceStatus
{
    public static void Recalculate(Domain.Entities.Invoice invoice)
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
