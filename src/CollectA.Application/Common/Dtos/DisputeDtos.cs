using CollectA.Domain.Enums;
using MediatR;

namespace CollectA.Application.Common.Dtos;

public class DisputeDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public DisputeType Type { get; set; }
    public string TypeLabel => Type.ToString();
    public DisputeStatus Status { get; set; }
    public string StatusLabel => Status.ToString();
    public decimal? DisputedAmount { get; set; }
    public string Currency { get; set; } = "DZD";
    public Guid? ResponsibleId { get; set; }
    public string? ResponsibleName { get; set; }
    public string? Department { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDisputeCommand : IRequest<DisputeDto>
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? InvoiceId { get; set; }
    public DisputeType Type { get; set; } = DisputeType.Other;
    public decimal? DisputedAmount { get; set; }
    public string Currency { get; set; } = "DZD";
    public Guid? ResponsibleId { get; set; }
    public string? Department { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateDisputeCommand : IRequest<DisputeDto>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DisputeType Type { get; set; }
    public DisputeStatus Status { get; set; }
    public decimal? DisputedAmount { get; set; }
    public Guid? ResponsibleId { get; set; }
    public string? Department { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
}

public class ChangeDisputeStatusCommand : IRequest<DisputeDto>
{
    public Guid Id { get; set; }
    public DisputeStatus Status { get; set; }
    public string? ResolutionNotes { get; set; }
}
