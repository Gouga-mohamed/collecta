using CollectA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectA.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Action).HasMaxLength(50).IsRequired();
        builder.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.UserEmail).HasMaxLength(256);
        builder.Property(a => a.IpAddress).HasMaxLength(50);
        builder.HasIndex(a => new { a.TenantId, a.CreatedAt });
    }
}
