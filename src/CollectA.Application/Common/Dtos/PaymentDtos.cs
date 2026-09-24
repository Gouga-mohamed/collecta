using CollectA.Domain.Enums;

namespace CollectA.Application.Common.Dtos;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "DZD";
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; }
    public string? Reference { get; set; }
    public string? BankName { get; set; }
    public string? Notes { get; set; }
    public ChequeDto? Cheque { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ChequeDto
{
    public Guid Id { get; set; }
    public string ChequeNumber { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? Drawer { get; set; }
    public DateTime? DueDate { get; set; }
    public ChequeStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "DZD";
    public string? Notes { get; set; }
}

public class CreateChequeCommand
{
    public string ChequeNumber { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? Drawer { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "DZD";
    public string? Notes { get; set; }
}
