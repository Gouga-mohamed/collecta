using CollectA.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CollectA.Infrastructure.Persistence;

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? CurrentTenantId
    {
        get
        {
            var tenantClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : null;
        }
    }

    public string? CurrentTenantSubdomain
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_subdomain")?.Value;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public Guid? CurrentUserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
