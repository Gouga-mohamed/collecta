using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.CollectionTasks;

public class GetCollectionTaskByIdQuery : IRequest<CollectionTaskDto?>
{
    public Guid Id { get; set; }
}

public class GetCollectionTaskByIdQueryHandler : IRequestHandler<GetCollectionTaskByIdQuery, CollectionTaskDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public GetCollectionTaskByIdQueryHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<CollectionTaskDto?> Handle(GetCollectionTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _context.CollectionTasks
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(t => t.Customer)
            .Include(t => t.Invoice)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null) return null;

        var assignedToName = task.AssignedToId.HasValue
            ? await _userLookupService.GetUserFullNameAsync(task.AssignedToId.Value, cancellationToken)
            : null;

        return GetCollectionTasksQueryHandler.MapToDto(task, task.Customer.Name, task.Invoice?.InvoiceNumber, assignedToName);
    }
}
