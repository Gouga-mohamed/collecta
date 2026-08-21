using CollectA.Domain.Common;

namespace CollectA.Domain.Entities;

public class ExchangeRate : BaseEntity
{
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Source { get; set; }
}
