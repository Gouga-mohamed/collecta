using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.CollectionTasks;

public class DeleteCollectionTaskCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteCollectionTaskCommandHandler : IRequestHandler<DeleteCollectionTaskCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public DeleteCollectionTaskCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task Handle(DeleteCollectionTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.CollectionTasks
            .ForTenant(_tenantContext)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null) throw new KeyNotFoundException($"Collection task '{request.Id}' not found.");

        _context.CollectionTasks.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
