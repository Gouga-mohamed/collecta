using CollectA.Domain.Enums;

namespace CollectA.Application.Common.Dtos;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount => Amount - PaidAmount;
    public string Currency { get; set; } = "DZD";
    public InvoiceStatus Status { get; set; }
    public string? Description { get; set; }
    public int PaymentTermsDays { get; set; }
    public bool IsDisputed { get; set; }
    public int DaysOverdue { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class InvoiceDetailDto : InvoiceDto
{
    public List<InvoiceLineDto> Lines { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
}

public class InvoiceLineDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}

public class CreateInvoiceLineCommand
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
