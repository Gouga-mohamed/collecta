using CollectA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectA.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => t.Subdomain).IsUnique();
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Subdomain).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Currency).HasMaxLength(3).IsRequired();
        builder.Property(t => t.Language).HasMaxLength(10).IsRequired();
        builder.Property(t => t.TimeZone).HasMaxLength(50).IsRequired();
    }
}
