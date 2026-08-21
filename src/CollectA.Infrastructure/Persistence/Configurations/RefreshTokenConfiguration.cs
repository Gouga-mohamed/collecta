using CollectA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectA.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Token).HasMaxLength(500).IsRequired();
        builder.HasIndex(r => new { r.TenantId, r.Token });
        builder.HasIndex(r => r.ExpiresAt);
    }
}
