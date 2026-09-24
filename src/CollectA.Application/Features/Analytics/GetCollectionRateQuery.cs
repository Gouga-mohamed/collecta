using CollectA.Domain.Common.Interfaces;
using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Analytics;

public class GetCollectionRateQuery : IRequest<CollectionRateDto>
{
    public int PeriodDays { get; set; } = 30;
}

public class GetCollectionRateQueryHandler : IRequestHandler<GetCollectionRateQuery, CollectionRateDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public GetCollectionRateQueryHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<CollectionRateDto> Handle(GetCollectionRateQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-request.PeriodDays);

        var invoicedAmount = await _context.Invoices
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= today)
            .SumAsync(i => i.Amount, cancellationToken);

        var collectedAmount = await _context.Payments
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= today)
            .SumAsync(p => p.Amount, cancellationToken);

        var rate = invoicedAmount > 0 ? collectedAmount / invoicedAmount * 100 : 0;

        return new CollectionRateDto
        {
            Rate = rate,
            InvoicedAmount = invoicedAmount,
            CollectedAmount = collectedAmount,
            Period = $"Last {request.PeriodDays} days"
        };
    }
}
