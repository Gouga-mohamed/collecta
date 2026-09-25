using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Promises;

public class GetPromiseToPayByIdQuery : IRequest<PromiseToPayDto?>
{
    public Guid Id { get; set; }
}

public class GetPromiseToPayByIdQueryHandler : IRequestHandler<GetPromiseToPayByIdQuery, PromiseToPayDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public GetPromiseToPayByIdQueryHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<PromiseToPayDto?> Handle(GetPromiseToPayByIdQuery request, CancellationToken cancellationToken)
    {
        var promise = await _context.PromiseToPays
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(p => p.Customer)
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (promise == null) return null;

        var responsibleName = promise.ResponsibleAgentId.HasValue
            ? await _userLookupService.GetUserFullNameAsync(promise.ResponsibleAgentId.Value, cancellationToken)
            : null;

        return GetPromisesQueryHandler.MapToDto(promise, promise.Customer.Name, promise.Invoice?.InvoiceNumber, responsibleName);
    }
}
