using CollectA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectA.Infrastructure.Persistence.Configurations;

public class DisputeConfiguration : IEntityTypeConfiguration<Dispute>
{
    public void Configure(EntityTypeBuilder<Dispute> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Title).HasMaxLength(200).IsRequired();
        builder.Property(d => d.DisputedAmount).HasPrecision(18, 2);
        builder.Property(d => d.Currency).HasMaxLength(3).IsRequired();
        builder.Property(d => d.Department).HasMaxLength(100);
        builder.HasOne(d => d.Customer).WithMany(c => c.Disputes).HasForeignKey(d => d.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(d => d.Invoice).WithMany(i => i.Disputes).HasForeignKey(d => d.InvoiceId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(d => d.ResponsibleId);
    }
}
