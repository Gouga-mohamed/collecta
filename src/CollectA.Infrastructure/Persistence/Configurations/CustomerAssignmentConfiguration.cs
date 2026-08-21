using CollectA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollectA.Infrastructure.Persistence.Configurations;

public class CustomerAssignmentConfiguration : IEntityTypeConfiguration<CustomerAssignment>
{
    public void Configure(EntityTypeBuilder<CustomerAssignment> builder)
    {
        builder.HasKey(ca => ca.Id);
        builder.Property(ca => ca.Role).HasMaxLength(50).IsRequired();
        builder.HasIndex(ca => new { ca.TenantId, ca.CustomerId, ca.UserId, ca.Role }).IsUnique();
        builder.HasOne(ca => ca.Customer).WithMany(c => c.Assignments).HasForeignKey(ca => ca.CustomerId).OnDelete(DeleteBehavior.Cascade);
    }
}
