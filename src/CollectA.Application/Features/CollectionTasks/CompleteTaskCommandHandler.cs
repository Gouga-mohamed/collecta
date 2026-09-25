using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskStatus = CollectA.Domain.Enums.TaskStatus;

namespace CollectA.Application.Features.CollectionTasks;

public class CompleteTaskCommand : IRequest<CollectionTaskDto>
{
    public Guid Id { get; set; }
    public string? CompletedNotes { get; set; }
}

public class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, CollectionTaskDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public CompleteTaskCommandHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<CollectionTaskDto> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.CollectionTasks
            .ForTenant(_tenantContext)
            .Include(t => t.Customer)
            .Include(t => t.Invoice)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null) throw new KeyNotFoundException($"Collection task '{request.Id}' not found.");

        task.Status = TaskStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.CompletedNotes))
        {
            task.CompletedNotes = request.CompletedNotes;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var assignedToName = task.AssignedToId.HasValue
            ? await _userLookupService.GetUserFullNameAsync(task.AssignedToId.Value, cancellationToken)
            : null;

        return GetCollectionTasksQueryHandler.MapToDto(task, task.Customer.Name, task.Invoice?.InvoiceNumber, assignedToName);
    }
}
