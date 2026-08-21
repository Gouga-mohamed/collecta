using CollectA.Domain.Common;
using CollectA.Domain.Enums;

namespace CollectA.Domain.Entities;

public class RiskScore : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int Score { get; set; } // 0-100
    public RiskLevel Level { get; set; }
    public string? Reason { get; set; }
    public decimal OverdueRatio { get; set; }
    public decimal AveragePaymentDelay { get; set; }
    public int BrokenPromisesCount { get; set; }
    public int DisputesCount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
