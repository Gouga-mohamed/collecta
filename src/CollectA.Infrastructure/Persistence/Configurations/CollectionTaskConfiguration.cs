using CollectA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectA.Infrastructure.Persistence.Configurations;

public class CollectionTaskConfiguration : IEntityTypeConfiguration<CollectionTask>
{
    public void Configure(EntityTypeBuilder<CollectionTask> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.HasOne(t => t.Customer).WithMany(c => c.Tasks).HasForeignKey(t => t.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.Invoice).WithMany().HasForeignKey(t => t.InvoiceId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(t => t.AssignedToId);
        builder.HasIndex(t => new { t.Status, t.DueDate });
    }
}
