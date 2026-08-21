using CollectA.Domain.Common;
using CollectA.Domain.Enums;

namespace CollectA.Domain.Entities;

public class CollectionAction : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public CollectionActionType Type { get; set; } = CollectionActionType.PhoneCall;
    public Guid? AssignedToId { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime ActionDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public string Notes { get; set; } = string.Empty;
    public CollectionActionOutcome Outcome { get; set; } = CollectionActionOutcome.None;
    public string? OutcomeNotes { get; set; }
    public bool IsClosed { get; set; } = false;
    public DateTime? ClosedAt { get; set; }
}
