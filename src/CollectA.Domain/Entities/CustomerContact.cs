using CollectA.Domain.Common;

namespace CollectA.Domain.Entities;

public class CustomerContact : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsPrimary { get; set; } = false;
    public bool IsBillingContact { get; set; } = false;
}
