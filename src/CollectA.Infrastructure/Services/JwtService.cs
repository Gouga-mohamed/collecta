using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CollectA.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CollectA.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TokenResult GenerateAccessToken(Guid userId, string email, string fullName, Guid tenantId, string tenantSubdomain, IEnumerable<string> permissions)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, fullName),
            new("tenant_id", tenantId.ToString()),
            new("tenant_subdomain", tenantSubdomain)
        };

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new TokenResult
        {
            AccessToken = tokenHandler.WriteToken(token),
            ExpiresAt = expires
        };
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
