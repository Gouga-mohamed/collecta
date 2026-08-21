using CollectA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectA.Infrastructure.Persistence.Configurations;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Content).HasMaxLength(4000).IsRequired();
        builder.Property(n => n.EntityType).HasMaxLength(100);
        builder.HasIndex(n => new { n.EntityType, n.EntityId });
    }
}
