using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common;
using CollectA.Domain.Entities;
using CollectA.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid,
    IdentityUserClaim<Guid>, ApplicationUserRole, IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>, IApplicationDbContext
{
    private readonly ITenantContext _tenantContext;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();
    public DbSet<CustomerAssignment> CustomerAssignments => Set<CustomerAssignment>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Cheque> Cheques => Set<Cheque>();
    public DbSet<CollectionAction> CollectionActions => Set<CollectionAction>();
    public DbSet<CollectionTask> CollectionTasks => Set<CollectionTask>();
    public DbSet<PromiseToPay> PromiseToPays => Set<PromiseToPay>();
    public DbSet<Dispute> Disputes => Set<Dispute>();
    public DbSet<RiskScore> RiskScores => Set<RiskScore>();
    public DbSet<CreditLimit> CreditLimits => Set<CreditLimit>();
    public DbSet<ReminderTemplate> ReminderTemplates => Set<ReminderTemplate>();
    public DbSet<ReminderRule> ReminderRules => Set<ReminderRule>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Integration> Integrations => Set<Integration>();
    public DbSet<IntegrationLog> IntegrationLogs => Set<IntegrationLog>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Tenant isolation is applied explicitly in application services and controllers.
        // Global query filters are intentionally not used here to avoid complex dynamic filter wiring
        // and to keep data access explicit and auditable.

        // Identity tenant isolation: users and roles are scoped to a tenant via TenantId.
        // Queries must always filter by TenantId. Indexes below support this.
        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();
            b.HasIndex(u => u.TenantId);
            b.HasOne(u => u.Tenant).WithMany().HasForeignKey(u => u.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApplicationRole>(b =>
        {
            b.Property(r => r.TenantId).IsRequired();
            b.HasIndex(r => new { r.TenantId, r.NormalizedName }).IsUnique();
            b.HasIndex(r => r.TenantId);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTenantId();
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetTenantId()
    {
        if (_tenantContext?.CurrentTenantId == null) return;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.Entity is Tenant) continue;

            if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
            {
                entry.Entity.TenantId = _tenantContext.CurrentTenantId.Value;
            }
        }
    }

    private void UpdateTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
