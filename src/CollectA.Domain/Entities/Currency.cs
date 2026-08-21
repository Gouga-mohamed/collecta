using CollectA.Domain.Common;

namespace CollectA.Domain.Entities;

public class Currency : BaseEntity
{
    public string Code { get; set; } = string.Empty; // DZD, EUR, USD
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
