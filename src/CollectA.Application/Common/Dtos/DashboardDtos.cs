using CollectA.Domain.Enums;

namespace CollectA.Application.Common.Dtos;

public class CfoDashboardDto
{
    public decimal TotalOutstanding { get; set; }
    public decimal TotalOverdue { get; set; }
    public decimal TotalCurrent { get; set; }
    public decimal TotalPaidThisMonth { get; set; }
    public decimal TotalPaidLastMonth { get; set; }
    public decimal OverduePercentage { get; set; }
    public decimal CollectionRate { get; set; }
    public decimal Dso { get; set; }
    public int OpenInvoicesCount { get; set; }
    public int OverdueInvoicesCount { get; set; }
    public int ActiveCustomersCount { get; set; }
    public int CustomersWithOverdueCount { get; set; }
    public List<AgingBucketDto> AgingBuckets { get; set; } = new();
    public List<TopDebtorDto> TopDebtors { get; set; } = new();
    public List<MonthlyCashDto> MonthlyCash { get; set; } = new();
    public List<UpcomingPromiseDto> UpcomingPromises { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class TopDebtorDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalDue { get; set; }
    public decimal TotalOverdue { get; set; }
    public RiskLevel RiskLevel { get; set; }
}

public class MonthlyCashDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Invoiced { get; set; }
    public decimal Collected { get; set; }
}

public class UpcomingPromiseDto
{
    public Guid PromiseId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal PromisedAmount { get; set; }
    public DateTime PromiseDate { get; set; }
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? CustomerName { get; set; }
}

public class CashForecastDto
{
    public List<CashForecastPointDto> Contractual { get; set; } = new();
    public List<CashForecastPointDto> Expected { get; set; } = new();
}

public class CashForecastPointDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class DsoResultDto
{
    public int DsoDays { get; set; }
    public decimal TotalOutstanding { get; set; }
    public decimal AverageDailySales { get; set; }
    public string Period { get; set; } = string.Empty;
}

public class CollectionRateDto
{
    public decimal Rate { get; set; }
    public decimal InvoicedAmount { get; set; }
    public decimal CollectedAmount { get; set; }
    public string Period { get; set; } = string.Empty;
}

public class CustomerRiskDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Score { get; set; }
    public RiskLevel Level { get; set; }
    public List<string> Factors { get; set; } = new();
    public DateTime CalculatedAt { get; set; }
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? EntityId { get; set; }
    public string? EntityType { get; set; }
}

public class ReminderTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Channel { get; set; }
    public bool IsBuiltIn { get; set; }
}

public class ReceivablesAnalyticsDto
{
    public decimal TotalOutstanding { get; set; }
    public decimal TotalOverdue { get; set; }
    public decimal TotalCurrent { get; set; }
    public decimal OverduePercentage { get; set; }
    public decimal AverageDaysOverdue { get; set; }
    public int OpenInvoicesCount { get; set; }
    public int OverdueInvoicesCount { get; set; }
    public List<AgingBucketDto> AgingBuckets { get; set; } = new();
    public List<ReceivablesTrendPointDto> Trends { get; set; } = new();
    public List<CustomerReceivableDto> TopDebtors { get; set; } = new();
}

public class ReceivablesTrendPointDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Outstanding { get; set; }
    public decimal Overdue { get; set; }
}

public class CustomerReceivableDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalDue { get; set; }
    public decimal TotalOverdue { get; set; }
}

public class CollectionsAnalyticsDto
{
    public decimal TotalCollected { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal CollectionRate { get; set; }
    public decimal AveragePaymentDelay { get; set; }
    public List<CollectionTrendPointDto> Trends { get; set; } = new();
    public List<CustomerCollectionDto> TopPayers { get; set; } = new();
}

public class CollectionTrendPointDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Invoiced { get; set; }
    public decimal Collected { get; set; }
}

public class CustomerCollectionDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Collected { get; set; }
    public decimal Invoiced { get; set; }
}
