using CollectA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectA.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => new { c.TenantId, c.Code }).IsUnique();
        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.LegalName).HasMaxLength(200);
        builder.Property(c => c.TradeRegister).HasMaxLength(100);
        builder.Property(c => c.TaxId).HasMaxLength(100);
        builder.Property(c => c.Industry).HasMaxLength(100);
        builder.Property(c => c.Country).HasMaxLength(100);
        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.Email).HasMaxLength(256);
        builder.HasOne(c => c.Tenant).WithMany(t => t.Customers).HasForeignKey(c => c.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
