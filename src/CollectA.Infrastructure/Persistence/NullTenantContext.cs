using CollectA.Domain.Common.Interfaces;
using CollectA.Application.Common.Interfaces;

namespace CollectA.Infrastructure.Persistence;

public class NullTenantContext : ITenantContext
{
    public Guid? CurrentTenantId => null;
    public string? CurrentTenantSubdomain => null;
    public bool IsAuthenticated => false;
    public Guid? CurrentUserId => null;
}
