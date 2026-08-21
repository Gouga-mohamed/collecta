using CollectA.Domain.Common;

namespace CollectA.Domain.Entities;

public class IntegrationLog : BaseEntity
{
    public Guid IntegrationId { get; set; }
    public Integration Integration { get; set; } = null!;
    public string Level { get; set; } = string.Empty; // Info, Warning, Error
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
}
