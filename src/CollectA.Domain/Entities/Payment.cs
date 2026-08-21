using CollectA.Domain.Common;
using CollectA.Domain.Enums;

namespace CollectA.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "DZD";
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;
    public string? Reference { get; set; }
    public string? BankName { get; set; }
    public string? Notes { get; set; }
    public Guid? ChequeId { get; set; }
    public Cheque? Cheque { get; set; }
}
