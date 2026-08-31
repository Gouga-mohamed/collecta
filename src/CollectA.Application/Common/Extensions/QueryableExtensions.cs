using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common;

namespace CollectA.Application.Common.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ForTenant<T>(this IQueryable<T> query, ITenantContext tenantContext)
        where T : BaseEntity
    {
        if (tenantContext.CurrentTenantId.HasValue)
        {
            return query.Where(e => e.TenantId == tenantContext.CurrentTenantId.Value);
        }

        return query;
    }
}
