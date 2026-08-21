using CollectA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectA.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        builder.Property(p => p.Reference).HasMaxLength(200);
        builder.Property(p => p.BankName).HasMaxLength(100);
        builder.HasOne(p => p.Customer).WithMany(c => c.Payments).HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Invoice).WithMany(i => i.Payments).HasForeignKey(p => p.InvoiceId).OnDelete(DeleteBehavior.SetNull);
    }
}
