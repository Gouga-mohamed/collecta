using CollectA.Domain.Enums;

namespace CollectA.Application.Common.Dtos;

public class PromiseToPayDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal PromisedAmount { get; set; }
    public string Currency { get; set; } = "DZD";
    public DateTime PromiseDate { get; set; }
    public Guid? ResponsibleAgentId { get; set; }
    public string? ResponsibleAgentName { get; set; }
    public PromiseStatus Status { get; set; }
    public decimal? FulfilledAmount { get; set; }
    public DateTime? FulfilledDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePromiseCommand
{
    public Guid CustomerId { get; set; }
    public Guid? InvoiceId { get; set; }
    public decimal PromisedAmount { get; set; }
    public string Currency { get; set; } = "DZD";
    public DateTime PromiseDate { get; set; }
    public Guid? ResponsibleAgentId { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePromiseCommand
{
    public Guid Id { get; set; }
    public decimal PromisedAmount { get; set; }
    public DateTime PromiseDate { get; set; }
    public PromiseStatus Status { get; set; }
    public decimal? FulfilledAmount { get; set; }
    public DateTime? FulfilledDate { get; set; }
    public Guid? ResponsibleAgentId { get; set; }
    public string? Notes { get; set; }
}
