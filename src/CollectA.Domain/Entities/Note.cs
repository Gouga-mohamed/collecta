using CollectA.Domain.Common;

namespace CollectA.Domain.Entities;

public class Note : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public Guid CreatedById { get; set; }
}
