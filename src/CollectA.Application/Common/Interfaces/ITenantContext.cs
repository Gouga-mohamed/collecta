namespace CollectA.Application.Common.Interfaces;

public interface ITenantContext
{
    Guid? CurrentTenantId { get; }
    string? CurrentTenantSubdomain { get; }
    bool IsAuthenticated { get; }
    Guid? CurrentUserId { get; }
}
