using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Domain.Common;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Enums;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Receivables;

public class GetAgingQuery : IRequest<List<AgingBucketDto>>
{
}

public class GetAgingQueryHandler : IRequestHandler<GetAgingQuery, List<AgingBucketDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public GetAgingQueryHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<List<AgingBucketDto>> Handle(GetAgingQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _context.Invoices
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.WrittenOff && (i.Amount - i.PaidAmount) > 0)
            .ToListAsync(cancellationToken);

        var buckets = new[]
        {
            new AgingBucketDto { Label = AgingCalculator.Current, MinDays = int.MinValue, MaxDays = 0 },
            new AgingBucketDto { Label = AgingCalculator.Days1To30, MinDays = 1, MaxDays = 30 },
            new AgingBucketDto { Label = AgingCalculator.Days31To60, MinDays = 31, MaxDays = 60 },
            new AgingBucketDto { Label = AgingCalculator.Days61To90, MinDays = 61, MaxDays = 90 },
            new AgingBucketDto { Label = AgingCalculator.Days91To120, MinDays = 91, MaxDays = 120 },
            new AgingBucketDto { Label = AgingCalculator.Days121To180, MinDays = 121, MaxDays = 180 },
            new AgingBucketDto { Label = AgingCalculator.Over180, MinDays = 181, MaxDays = int.MaxValue }
        };

        foreach (var invoice in invoices)
        {
            var bucketLabel = AgingCalculator.GetBucket(invoice.DaysOverdue);
            var bucket = buckets.First(b => b.Label == bucketLabel);
            bucket.Amount += invoice.RemainingAmount;
            bucket.InvoiceCount++;
        }

        return buckets.ToList();
    }
}
