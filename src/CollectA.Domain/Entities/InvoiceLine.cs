using CollectA.Domain.Common;

namespace CollectA.Domain.Entities;

public class InvoiceLine : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Amount => Quantity * UnitPrice;
}
