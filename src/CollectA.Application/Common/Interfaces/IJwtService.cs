using CollectA.Application.Common.Interfaces;

namespace CollectA.Application.Common.Interfaces;

public interface IJwtService
{
    TokenResult GenerateAccessToken(Guid userId, string email, string fullName, Guid tenantId, string tenantSubdomain, IEnumerable<string> permissions);
    string GenerateRefreshToken();
}

public class TokenResult
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
