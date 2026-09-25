using CollectA.Domain.Enums;
using MediatR;
using TaskStatus = CollectA.Domain.Enums.TaskStatus;

namespace CollectA.Application.Common.Dtos;

public class CollectionTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskStatus Status { get; set; }
    public string StatusLabel => Status.ToString();
    public Priority Priority { get; set; }
    public string PriorityLabel => Priority.ToString();
    public DateTime? CompletedAt { get; set; }
    public string? CompletedNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCollectionTaskCommand : IRequest<CollectionTaskDto>
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
}

public class UpdateCollectionTaskCommand : IRequest<CollectionTaskDto>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskStatus Status { get; set; }
    public Priority Priority { get; set; }
    public string? CompletedNotes { get; set; }
}

public class CollectionActionDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public CollectionActionType Type { get; set; }
    public string TypeLabel => Type.ToString();
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Priority Priority { get; set; }
    public string PriorityLabel => Priority.ToString();
    public string Notes { get; set; } = string.Empty;
    public CollectionActionOutcome Outcome { get; set; }
    public string OutcomeLabel => Outcome.ToString();
    public string? OutcomeNotes { get; set; }
    public bool IsClosed { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCollectionActionCommand : IRequest<CollectionActionDto>
{
    public Guid CustomerId { get; set; }
    public Guid? InvoiceId { get; set; }
    public CollectionActionType Type { get; set; } = CollectionActionType.PhoneCall;
    public Guid? AssignedToId { get; set; }
    public DateTime ActionDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public string Notes { get; set; } = string.Empty;
}

public class UpdateCollectionActionCommand : IRequest<CollectionActionDto>
{
    public Guid Id { get; set; }
    public CollectionActionOutcome Outcome { get; set; }
    public string? OutcomeNotes { get; set; }
    public bool IsClosed { get; set; }
    public DateTime? ClosedAt { get; set; }
    public Guid? AssignedToId { get; set; }
}

public class CloseCollectionActionCommand : IRequest<CollectionActionDto>
{
    public Guid Id { get; set; }
}

public class AgentDashboardDto
{
    public int PendingTasksCount { get; set; }
    public int TodayActionsCount { get; set; }
    public int DuePromisesCount { get; set; }
    public int OverdueTasksCount { get; set; }
    public List<CollectionTaskDto> MyTasks { get; set; } = new();
    public List<CollectionActionDto> TodayActions { get; set; } = new();
    public List<PromiseToPayDto> DuePromises { get; set; } = new();
}
