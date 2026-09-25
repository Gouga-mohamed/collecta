using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Promises;

public class FulfillPromiseToPayCommandHandler : IRequestHandler<FulfillPromiseToPayCommand, PromiseToPayDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public FulfillPromiseToPayCommandHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<PromiseToPayDto> Handle(FulfillPromiseToPayCommand request, CancellationToken cancellationToken)
    {
        var promise = await _context.PromiseToPays
            .ForTenant(_tenantContext)
            .Include(p => p.Customer)
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (promise == null) throw new KeyNotFoundException($"Promise to pay '{request.Id}' not found.");

        if (promise.Status == PromiseStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot fulfill a cancelled promise.");
        }

        promise.FulfilledAmount = request.FulfilledAmount;
        promise.FulfilledDate = DateTime.UtcNow;
        promise.Status = PromiseStatusCalculator.Calculate(promise, request.FulfilledAmount);

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            promise.Notes = request.Notes;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var responsibleName = promise.ResponsibleAgentId.HasValue
            ? await _userLookupService.GetUserFullNameAsync(promise.ResponsibleAgentId.Value, cancellationToken)
            : null;

        return GetPromisesQueryHandler.MapToDto(promise, promise.Customer.Name, promise.Invoice?.InvoiceNumber, responsibleName);
    }
}
