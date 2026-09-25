using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Disputes;

public class GetDisputeByIdQuery : IRequest<DisputeDto?>
{
    public Guid Id { get; set; }
}

public class GetDisputeByIdQueryHandler : IRequestHandler<GetDisputeByIdQuery, DisputeDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public GetDisputeByIdQueryHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<DisputeDto?> Handle(GetDisputeByIdQuery request, CancellationToken cancellationToken)
    {
        var dispute = await _context.Disputes
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(d => d.Customer)
            .Include(d => d.Invoice)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (dispute == null) return null;

        var responsibleName = dispute.ResponsibleId.HasValue
            ? await _userLookupService.GetUserFullNameAsync(dispute.ResponsibleId.Value, cancellationToken)
            : null;

        return GetDisputesQueryHandler.MapToDto(dispute, dispute.Customer.Name, dispute.Invoice?.InvoiceNumber, responsibleName);
    }
}
