namespace CollectA.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, Guid? entityId, string? beforeJson, string? afterJson, CancellationToken cancellationToken = default);
}
