using CollectA.Domain.Common;
using CollectA.Domain.Enums;

namespace CollectA.Domain.Entities;

public class CollectionTask : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public Guid? AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Pending;
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime? CompletedAt { get; set; }
    public string? CompletedNotes { get; set; }
}
