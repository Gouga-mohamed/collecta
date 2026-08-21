using CollectA.Domain.Common;
using CollectA.Domain.Enums;

namespace CollectA.Domain.Entities;

public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount => Amount - PaidAmount;
    public Tenant Tenant { get; set; } = null!;
    public string Currency { get; set; } = "DZD";
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Open;
    public string? Description { get; set; }
    public string? SalespersonId { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    public bool IsDisputed { get; set; } = false;
    public DateTime? DisputedAt { get; set; }
    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<CollectionAction> CollectionActions { get; set; } = new List<CollectionAction>();
    public ICollection<PromiseToPay> Promises { get; set; } = new List<PromiseToPay>();
    public ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();
    public int DaysOverdue => DueDate < DateTime.UtcNow.Date && RemainingAmount > 0
        ? (DateTime.UtcNow.Date - DueDate).Days
        : 0;
    public bool IsOverdue => DaysOverdue > 0;
}
