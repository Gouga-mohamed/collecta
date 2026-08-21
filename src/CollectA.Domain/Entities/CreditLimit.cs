using CollectA.Domain.Common;

namespace CollectA.Domain.Entities;

public class CreditLimit : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public decimal Limit { get; set; }
    public string Currency { get; set; } = "DZD";
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}
