using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Entities;
using CollectA.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CollectA.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string action, string entityType, Guid? entityId, string? beforeJson, string? afterJson, CancellationToken cancellationToken = default)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = user?.FindFirst(ClaimTypes.Email)?.Value;
        var tenantId = user?.FindFirst("tenant_id")?.Value;

        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = Guid.TryParse(userId, out var uid) ? uid : null,
            UserEmail = email,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            BeforeJson = beforeJson,
            AfterJson = afterJson,
            IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString(),
            TenantId = Guid.TryParse(tenantId, out var tid) ? tid : Guid.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
