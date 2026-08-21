using CollectA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectA.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
        builder.Property(n => n.EntityType).HasMaxLength(100);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
    }
}
