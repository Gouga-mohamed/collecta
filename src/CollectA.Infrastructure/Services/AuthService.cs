using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Entities;
using CollectA.Infrastructure.Identity;
using CollectA.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public AuthService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService)
    {
        _context = context;
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var result = new AuthResult();

        if (await _context.Tenants.AnyAsync(t => t.Subdomain == request.TenantSubdomain, cancellationToken))
        {
            result.Errors.Add("Tenant subdomain already exists.");
            return result;
        }

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.TenantName,
            Subdomain = request.TenantSubdomain.ToLowerInvariant(),
            Currency = "DZD",
            Language = "fr",
            TimeZone = "Africa/Algiers"
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            NormalizedEmail = request.Email.ToUpperInvariant(),
            NormalizedUserName = request.Email.ToUpperInvariant(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            TenantId = tenant.Id,
            EmailConfirmed = true,
            LockoutEnabled = false
        };

        var identityResult = await _userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            result.Errors.AddRange(identityResult.Errors.Select(e => e.Description));
            return result;
        }

        await EnsureRoleAsync(tenant.Id, "Owner", new[]
        {
            "customers.read", "customers.write",
            "invoices.read", "invoices.write",
            "payments.read", "payments.write",
            "collections.read", "collections.write",
            "promises.read", "promises.write",
            "disputes.read", "disputes.write",
            "analytics.read",
            "settings.read", "settings.write",
            "users.read", "users.write"
        }, cancellationToken);

        await _userManager.AddToRoleAsync(user, "Owner");

        var permissions = await GetUserPermissionsAsync(user.Id, cancellationToken);
        var tokenResult = _jwtService.GenerateAccessToken(
            user.Id, user.Email!, user.FullName, tenant.Id, tenant.Subdomain, permissions);
        var refreshToken = await CreateRefreshTokenAsync(user.Id, tenant.Id, cancellationToken);

        result.Succeeded = true;
        result.AccessToken = tokenResult.AccessToken;
        result.RefreshToken = refreshToken;
        result.ExpiresAt = tokenResult.ExpiresAt;
        result.User = await BuildUserProfileAsync(user, permissions, cancellationToken);

        return result;
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var result = new AuthResult();
        var user = await _userManager.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive, cancellationToken);

        if (user == null)
        {
            result.Errors.Add("Invalid credentials.");
            return result;
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            result.Errors.Add("Invalid credentials.");
            return result;
        }

        var permissions = await GetUserPermissionsAsync(user.Id, cancellationToken);
        var tokenResult = _jwtService.GenerateAccessToken(
            user.Id, user.Email!, user.FullName, user.TenantId, user.Tenant.Subdomain, permissions);
        var refreshToken = await CreateRefreshTokenAsync(user.Id, user.TenantId, cancellationToken);

        result.Succeeded = true;
        result.AccessToken = tokenResult.AccessToken;
        result.RefreshToken = refreshToken;
        result.ExpiresAt = tokenResult.ExpiresAt;
        result.User = await BuildUserProfileAsync(user, permissions, cancellationToken);

        return result;
    }

    public async Task<AuthResult> RefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var result = new AuthResult();
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token && r.IsActive, cancellationToken);

        if (refreshToken == null)
        {
            result.Errors.Add("Invalid refresh token.");
            return result;
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var user = await _userManager.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == refreshToken.UserId, cancellationToken);
        if (user == null)
        {
            result.Errors.Add("Invalid refresh token.");
            return result;
        }

        var permissions = await GetUserPermissionsAsync(user.Id, cancellationToken);
        var tokenResult = _jwtService.GenerateAccessToken(
            user.Id, user.Email!, user.FullName, user.TenantId, user.Tenant.Subdomain, permissions);
        var newRefreshToken = await CreateRefreshTokenAsync(user.Id, user.TenantId, cancellationToken);

        result.Succeeded = true;
        result.AccessToken = tokenResult.AccessToken;
        result.RefreshToken = newRefreshToken;
        result.ExpiresAt = tokenResult.ExpiresAt;
        result.User = await BuildUserProfileAsync(user, permissions, cancellationToken);

        return result;
    }

    public async Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token, cancellationToken);
        if (refreshToken != null)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<UserProfile?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return null;

        var permissions = await GetUserPermissionsAsync(user.Id, cancellationToken);
        return await BuildUserProfileAsync(user, permissions, cancellationToken);
    }

    private Task<UserProfile> BuildUserProfileAsync(ApplicationUser user, List<string> permissions, CancellationToken cancellationToken)
    {
        return Task.FromResult(new UserProfile
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            JobTitle = user.JobTitle,
            AvatarUrl = user.AvatarUrl,
            TenantId = user.TenantId,
            TenantName = user.Tenant?.Name ?? string.Empty,
            TenantSubdomain = user.Tenant?.Subdomain ?? string.Empty,
            Permissions = permissions
        });
    }

    private async Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        return await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task EnsureRoleAsync(Guid tenantId, string roleName, string[] permissions, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == tenantId, cancellationToken);
        if (role == null)
        {
            role = new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                TenantId = tenantId,
                IsBuiltIn = true
            };
            _context.Roles.Add(role);
        }

        foreach (var permissionName in permissions)
        {
            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Name == permissionName, cancellationToken);
            if (permission == null)
            {
                permission = new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = permissionName,
                    Category = permissionName.Split('.')[0]
                };
                _context.Permissions.Add(permission);
            }

            if (!await _context.RolePermissions.AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id, cancellationToken))
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> CreateRefreshTokenAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken)
    {
        var token = _jwtService.GenerateRefreshToken();
        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);
        return token;
    }
}
