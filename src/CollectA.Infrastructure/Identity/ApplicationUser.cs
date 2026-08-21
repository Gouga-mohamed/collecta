using CollectA.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CollectA.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
}

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public bool IsBuiltIn { get; set; } = false;
    public Guid TenantId { get; set; }
    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class ApplicationUserRole : IdentityUserRole<Guid>
{
}

public class Permission
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class RolePermission
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public ApplicationRole Role { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
