using CollectA.Domain.Common;
using CollectA.Domain.Enums;

namespace CollectA.Domain.Entities;

public class PromiseToPay : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public decimal PromisedAmount { get; set; }
    public string Currency { get; set; } = "DZD";
    public DateTime PromiseDate { get; set; }
    public Guid? ResponsibleAgentId { get; set; }
    public PromiseStatus Status { get; set; } = PromiseStatus.Pending;
    public decimal? FulfilledAmount { get; set; }
    public DateTime? FulfilledDate { get; set; }
    public string? Notes { get; set; }
}
