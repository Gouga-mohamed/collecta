using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.CollectionActions;

public class UpdateCollectionActionCommandHandler : IRequestHandler<UpdateCollectionActionCommand, CollectionActionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public UpdateCollectionActionCommandHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<CollectionActionDto> Handle(UpdateCollectionActionCommand request, CancellationToken cancellationToken)
    {
        var action = await _context.CollectionActions
            .ForTenant(_tenantContext)
            .Include(a => a.Customer)
            .Include(a => a.Invoice)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (action == null) throw new KeyNotFoundException($"Collection action '{request.Id}' not found.");

        action.Outcome = request.Outcome;
        action.OutcomeNotes = request.OutcomeNotes;
        action.AssignedToId = request.AssignedToId;

        if (request.IsClosed && !action.IsClosed)
        {
            action.IsClosed = true;
            action.ClosedAt = request.ClosedAt ?? DateTime.UtcNow;
        }
        else if (!request.IsClosed)
        {
            action.IsClosed = false;
            action.ClosedAt = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var assignedToName = action.AssignedToId.HasValue
            ? await _userLookupService.GetUserFullNameAsync(action.AssignedToId.Value, cancellationToken)
            : null;
        var createdByName = await _userLookupService.GetUserFullNameAsync(action.CreatedById, cancellationToken);

        return GetCollectionActionsQueryHandler.MapToDto(action, action.Customer.Name, action.Invoice?.InvoiceNumber, assignedToName, createdByName);
    }
}
