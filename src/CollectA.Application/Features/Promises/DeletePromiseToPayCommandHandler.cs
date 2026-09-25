using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Promises;

public class DeletePromiseToPayCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeletePromiseToPayCommandHandler : IRequestHandler<DeletePromiseToPayCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public DeletePromiseToPayCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task Handle(DeletePromiseToPayCommand request, CancellationToken cancellationToken)
    {
        var promise = await _context.PromiseToPays
            .ForTenant(_tenantContext)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (promise == null) throw new KeyNotFoundException($"Promise to pay '{request.Id}' not found.");

        _context.PromiseToPays.Remove(promise);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
