using CollectA.Domain.Common;
using CollectA.Domain.Enums;

namespace CollectA.Domain.Entities;

public class Cheque : BaseEntity
{
    public string ChequeNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string? BankName { get; set; }
    public string? Drawer { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "DZD";
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? DepositDate { get; set; }
    public ChequeStatus Status { get; set; } = ChequeStatus.Received;
    public string? Notes { get; set; }
    public Payment? Payment { get; set; }
}
