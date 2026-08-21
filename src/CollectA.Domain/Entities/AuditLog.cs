using CollectA.Domain.Common;

namespace CollectA.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, Login
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
