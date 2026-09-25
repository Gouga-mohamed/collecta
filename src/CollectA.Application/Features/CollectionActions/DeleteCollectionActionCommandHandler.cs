using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.CollectionActions;

public class DeleteCollectionActionCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteCollectionActionCommandHandler : IRequestHandler<DeleteCollectionActionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public DeleteCollectionActionCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task Handle(DeleteCollectionActionCommand request, CancellationToken cancellationToken)
    {
        var action = await _context.CollectionActions
            .ForTenant(_tenantContext)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (action == null) throw new KeyNotFoundException($"Collection action '{request.Id}' not found.");

        _context.CollectionActions.Remove(action);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
