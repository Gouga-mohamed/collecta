using CollectA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectA.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id);
        builder.HasIndex(i => new { i.TenantId, i.InvoiceNumber }).IsUnique();
        builder.Property(i => i.InvoiceNumber).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Currency).HasMaxLength(3).IsRequired();
        builder.Property(i => i.Amount).HasPrecision(18, 2);
        builder.Property(i => i.PaidAmount).HasPrecision(18, 2);
        builder.HasOne(i => i.Customer).WithMany(c => c.Invoices).HasForeignKey(i => i.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(i => i.Tenant).WithMany(t => t.Invoices).HasForeignKey(i => i.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}
