using CollectA.Application.Common.Interfaces;
using CollectA.Infrastructure.Identity;
using CollectA.Infrastructure.Persistence;
using CollectA.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CollectA.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}
