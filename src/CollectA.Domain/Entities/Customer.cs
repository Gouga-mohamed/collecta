using CollectA.Domain.Common;

namespace CollectA.Domain.Entities;

public class Customer : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TradeRegister { get; set; }
    public string? TaxId { get; set; }
    public string? Industry { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; } = "Algeria";
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public string? SalespersonId { get; set; }
    public string? CreditManagerId { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public ICollection<CustomerContact> Contacts { get; set; } = new List<CustomerContact>();
    public ICollection<CustomerAssignment> Assignments { get; set; } = new List<CustomerAssignment>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<CollectionAction> CollectionActions { get; set; } = new List<CollectionAction>();
    public ICollection<CollectionTask> Tasks { get; set; } = new List<CollectionTask>();
    public ICollection<PromiseToPay> Promises { get; set; } = new List<PromiseToPay>();
    public ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();
    public ICollection<RiskScore> RiskScores { get; set; } = new List<RiskScore>();
    public ICollection<CreditLimit> CreditLimits { get; set; } = new List<CreditLimit>();
}
