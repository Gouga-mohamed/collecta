using CollectA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectA.Infrastructure.Persistence.Configurations;

public class PromiseToPayConfiguration : IEntityTypeConfiguration<PromiseToPay>
{
    public void Configure(EntityTypeBuilder<PromiseToPay> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PromisedAmount).HasPrecision(18, 2);
        builder.Property(p => p.FulfilledAmount).HasPrecision(18, 2);
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        builder.HasOne(p => p.Customer).WithMany(c => c.Promises).HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Invoice).WithMany(i => i.Promises).HasForeignKey(p => p.InvoiceId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(p => p.ResponsibleAgentId);
    }
}
