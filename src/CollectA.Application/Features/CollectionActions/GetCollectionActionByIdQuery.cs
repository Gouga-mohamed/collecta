using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.CollectionActions;

public class GetCollectionActionByIdQuery : IRequest<CollectionActionDto?>
{
    public Guid Id { get; set; }
}

public class GetCollectionActionByIdQueryHandler : IRequestHandler<GetCollectionActionByIdQuery, CollectionActionDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public GetCollectionActionByIdQueryHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<CollectionActionDto?> Handle(GetCollectionActionByIdQuery request, CancellationToken cancellationToken)
    {
        var action = await _context.CollectionActions
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(a => a.Customer)
            .Include(a => a.Invoice)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (action == null) return null;

        var assignedToName = action.AssignedToId.HasValue
            ? await _userLookupService.GetUserFullNameAsync(action.AssignedToId.Value, cancellationToken)
            : null;
        var createdByName = await _userLookupService.GetUserFullNameAsync(action.CreatedById, cancellationToken);

        return GetCollectionActionsQueryHandler.MapToDto(action, action.Customer.Name, action.Invoice?.InvoiceNumber, assignedToName, createdByName);
    }
}
