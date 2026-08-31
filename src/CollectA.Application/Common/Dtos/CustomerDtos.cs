using CollectA.Domain.Enums;

namespace CollectA.Application.Common.Dtos;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? Industry { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int PaymentTermsDays { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalDue { get; set; }
    public decimal TotalOverdue { get; set; }
    public RiskLevel RiskLevel { get; set; }
}

public class CustomerDetailDto : CustomerDto
{
    public string? TradeRegister { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public List<CustomerContactDto> Contacts { get; set; } = new();
    public List<InvoiceDto> RecentInvoices { get; set; } = new();
    public List<PaymentDto> RecentPayments { get; set; } = new();
    public List<CollectionActionDto> RecentActions { get; set; } = new();
    public List<PromiseToPayDto> RecentPromises { get; set; } = new();
    public List<DisputeDto> RecentDisputes { get; set; } = new();
}

public class CustomerContactDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? JobTitle { get; set; }
    public bool IsPrimary { get; set; }
}

public class CreateCustomerCommand
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
    public int PaymentTermsDays { get; set; } = 30;
    public decimal CreditLimit { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCustomerCommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TradeRegister { get; set; }
    public string? TaxId { get; set; }
    public string? Industry { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public int PaymentTermsDays { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class Customer360Dto
{
    public CustomerDetailDto Customer { get; set; } = null!;
    public int OpenInvoicesCount { get; set; }
    public int OverdueInvoicesCount { get; set; }
    public decimal DsoApproximate { get; set; }
    public List<InvoiceDto> Invoices { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
    public List<CollectionActionDto> Actions { get; set; } = new();
    public List<PromiseToPayDto> Promises { get; set; } = new();
    public List<DisputeDto> Disputes { get; set; } = new();
    public List<NoteDto> Notes { get; set; } = new();
}

public class NoteDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}
