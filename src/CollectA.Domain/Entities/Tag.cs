using CollectA.Domain.Common;

namespace CollectA.Domain.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
}
