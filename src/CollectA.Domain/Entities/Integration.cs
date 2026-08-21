using CollectA.Domain.Common;
using CollectA.Domain.Enums;

namespace CollectA.Domain.Entities;

public class Integration : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public IntegrationType Type { get; set; }
    public string Provider { get; set; } = string.Empty;
    public IntegrationStatus Status { get; set; } = IntegrationStatus.Pending;
    public DateTime? LastSyncAt { get; set; }
    public string? ConfigurationJson { get; set; }
    public ICollection<IntegrationLog> Logs { get; set; } = new List<IntegrationLog>();
}
