using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Enums;
using MediatR;
using TaskStatus = CollectA.Domain.Enums.TaskStatus;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.CollectionTasks;

public class UpdateCollectionTaskCommandHandler : IRequestHandler<UpdateCollectionTaskCommand, CollectionTaskDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public UpdateCollectionTaskCommandHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<CollectionTaskDto> Handle(UpdateCollectionTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.CollectionTasks
            .ForTenant(_tenantContext)
            .Include(t => t.Customer)
            .Include(t => t.Invoice)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null) throw new KeyNotFoundException($"Collection task '{request.Id}' not found.");

        task.Title = request.Title;
        task.Description = request.Description;
        task.AssignedToId = request.AssignedToId;
        task.DueDate = request.DueDate;
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.CompletedNotes = request.CompletedNotes;

        if (request.Status == TaskStatus.Completed && !task.CompletedAt.HasValue)
        {
            task.CompletedAt = DateTime.UtcNow;
        }
        else if (request.Status != TaskStatus.Completed)
        {
            task.CompletedAt = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var assignedToName = task.AssignedToId.HasValue
            ? await _userLookupService.GetUserFullNameAsync(task.AssignedToId.Value, cancellationToken)
            : null;

        return GetCollectionTasksQueryHandler.MapToDto(task, task.Customer.Name, task.Invoice?.InvoiceNumber, assignedToName);
    }
}
