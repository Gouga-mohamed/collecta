using CollectA.Domain.Common;
using CollectA.Domain.Enums;

namespace CollectA.Domain.Entities;

public class Dispute : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public DisputeType Type { get; set; } = DisputeType.Other;
    public DisputeStatus Status { get; set; } = DisputeStatus.Open;
    public decimal? DisputedAmount { get; set; }
    public string Currency { get; set; } = "DZD";
    public Guid? ResponsibleId { get; set; }
    public string? Department { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
}
