using CollectA.Domain.Common;

namespace CollectA.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string Currency { get; set; } = "DZD";
    public string Language { get; set; } = "fr";
    public string TimeZone { get; set; } = "Africa/Algiers";
    public string? SubscriptionPlan { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
