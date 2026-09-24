using CollectA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectA.Infrastructure.Persistence.Configurations;

public class ChequeConfiguration : IEntityTypeConfiguration<Cheque>
{
    public void Configure(EntityTypeBuilder<Cheque> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.ChequeNumber).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => new { c.TenantId, c.ChequeNumber });
        builder.Property(c => c.Amount).HasPrecision(18, 2);
        builder.Property(c => c.Currency).HasMaxLength(3).IsRequired();
        builder.Property(c => c.BankName).HasMaxLength(100);
        builder.Property(c => c.Drawer).HasMaxLength(200);
        builder.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Cascade);
    }
}
