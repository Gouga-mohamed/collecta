using CollectA.Domain.Common;

namespace CollectA.Domain.Entities;

public class Document : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public Guid? UploadedById { get; set; }
}
