namespace CollectA.Application.Common.Dtos;

public class ReceivablesSummaryDto
{
    public decimal TotalOutstanding { get; set; }
    public decimal TotalOverdue { get; set; }
    public decimal TotalCurrent { get; set; }
    public decimal TotalPaidThisMonth { get; set; }
    public decimal OverduePercentage { get; set; }
    public int OpenInvoicesCount { get; set; }
    public int OverdueInvoicesCount { get; set; }
    public int CustomersWithOverdueCount { get; set; }
    public List<AgingBucketDto> AgingBuckets { get; set; } = new();
}

public class AgingBucketDto
{
    public string Label { get; set; } = string.Empty;
    public int MinDays { get; set; }
    public int MaxDays { get; set; }
    public decimal Amount { get; set; }
    public int InvoiceCount { get; set; }
}

public class OverdueInvoiceDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal RemainingAmount { get; set; }
    public int DaysOverdue { get; set; }
    public string Currency { get; set; } = "DZD";
}
