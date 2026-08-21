using CollectA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectA.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.FileName).HasMaxLength(500).IsRequired();
        builder.Property(d => d.OriginalFileName).HasMaxLength(500);
        builder.Property(d => d.ContentType).HasMaxLength(100);
        builder.Property(d => d.StoragePath).HasMaxLength(1000).IsRequired();
        builder.Property(d => d.EntityType).HasMaxLength(100);
        builder.HasIndex(d => new { d.EntityType, d.EntityId });
    }
}
