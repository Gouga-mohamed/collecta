using CollectA.Domain.Common;

namespace CollectA.Domain.Entities;

public class CustomerAssignment : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty; // Sales, CollectionAgent, CreditManager
}
