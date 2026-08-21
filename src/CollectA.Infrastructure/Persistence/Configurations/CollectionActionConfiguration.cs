using CollectA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectA.Infrastructure.Persistence.Configurations;

public class CollectionActionConfiguration : IEntityTypeConfiguration<CollectionAction>
{
    public void Configure(EntityTypeBuilder<CollectionAction> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Notes).HasMaxLength(2000);
        builder.Property(a => a.OutcomeNotes).HasMaxLength(1000);
        builder.HasOne(a => a.Customer).WithMany(c => c.CollectionActions).HasForeignKey(a => a.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Invoice).WithMany(i => i.CollectionActions).HasForeignKey(a => a.InvoiceId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(a => a.AssignedToId);
        builder.HasIndex(a => a.CreatedById);
    }
}
