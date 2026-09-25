using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskStatus = CollectA.Domain.Enums.TaskStatus;

namespace CollectA.Application.Features.Collections;

public class GetAgentDashboardQuery : IRequest<AgentDashboardDto>
{
}

public class GetAgentDashboardQueryHandler : IRequestHandler<GetAgentDashboardQuery, AgentDashboardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public GetAgentDashboardQueryHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<AgentDashboardDto> Handle(GetAgentDashboardQuery request, CancellationToken cancellationToken)
    {
        var userId = _tenantContext.CurrentUserId;
        var today = DateTime.UtcNow.Date;
        var endOfWeek = today.AddDays(7 - (int)today.DayOfWeek);

        var myTasksQuery = _context.CollectionTasks
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(t => t.Customer)
            .Include(t => t.Invoice)
            .Where(t => t.AssignedToId == userId && t.DueDate == today && t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled);

        var overdueTasksQuery = _context.CollectionTasks
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(t => t.Customer)
            .Include(t => t.Invoice)
            .Where(t => t.AssignedToId == userId && t.DueDate < today && t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled);

        var todaysActionsQuery = _context.CollectionActions
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(a => a.Customer)
            .Include(a => a.Invoice)
            .Where(a => a.AssignedToId == userId && a.ActionDate.Date == today && !a.IsClosed);

        var promisesDueThisWeekQuery = _context.PromiseToPays
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(p => p.Customer)
            .Include(p => p.Invoice)
            .Where(p => p.ResponsibleAgentId == userId && p.PromiseDate.Date <= endOfWeek && p.PromiseDate.Date >= today && p.Status == PromiseStatus.Pending);

        var myTasks = await myTasksQuery.ToListAsync(cancellationToken);
        var overdueTasks = await overdueTasksQuery.ToListAsync(cancellationToken);
        var todaysActions = await todaysActionsQuery.ToListAsync(cancellationToken);
        var promisesDueThisWeek = await promisesDueThisWeekQuery.ToListAsync(cancellationToken);

        var myTaskDtos = new List<CollectionTaskDto>();
        foreach (var task in myTasks)
        {
            var assignedToName = task.AssignedToId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(task.AssignedToId.Value, cancellationToken)
                : null;
            myTaskDtos.Add(Features.CollectionTasks.GetCollectionTasksQueryHandler.MapToDto(task, task.Customer.Name, task.Invoice?.InvoiceNumber, assignedToName));
        }

        var overdueTaskDtos = new List<CollectionTaskDto>();
        foreach (var task in overdueTasks)
        {
            var assignedToName = task.AssignedToId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(task.AssignedToId.Value, cancellationToken)
                : null;
            overdueTaskDtos.Add(Features.CollectionTasks.GetCollectionTasksQueryHandler.MapToDto(task, task.Customer.Name, task.Invoice?.InvoiceNumber, assignedToName));
        }

        var actionDtos = new List<CollectionActionDto>();
        foreach (var action in todaysActions)
        {
            var assignedToName = action.AssignedToId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(action.AssignedToId.Value, cancellationToken)
                : null;
            var createdByName = await _userLookupService.GetUserFullNameAsync(action.CreatedById, cancellationToken);
            actionDtos.Add(Features.CollectionActions.GetCollectionActionsQueryHandler.MapToDto(action, action.Customer.Name, action.Invoice?.InvoiceNumber, assignedToName, createdByName));
        }

        var promiseDtos = new List<PromiseToPayDto>();
        foreach (var promise in promisesDueThisWeek)
        {
            var responsibleName = promise.ResponsibleAgentId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(promise.ResponsibleAgentId.Value, cancellationToken)
                : null;
            promiseDtos.Add(Features.Promises.GetPromisesQueryHandler.MapToDto(promise, promise.Customer.Name, promise.Invoice?.InvoiceNumber, responsibleName));
        }

        return new AgentDashboardDto
        {
            PendingTasksCount = myTasks.Count,
            TodayActionsCount = todaysActions.Count,
            DuePromisesCount = promisesDueThisWeek.Count,
            OverdueTasksCount = overdueTasks.Count,
            MyTasks = myTaskDtos,
            TodayActions = actionDtos,
            DuePromises = promiseDtos
        };
    }
}
