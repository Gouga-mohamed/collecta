using CollectA.Application.Common.Dtos;
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
    private readonly IIIApplicationDbContext _context;

    public GetAgingQueryHandler(IIIApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AgingBucketDto>> Handle(GetAgingQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.WrittenOff && i.RemainingAmount > 0)
            .ToListAsync(cancellationToken);

        var buckets = new[]
        {
            new AgingBucketDto { Label = "Current", MinDays = int.MinValue, MaxDays = 0 },
            new AgingBucketDto { Label = "1-30 days", MinDays = 1, MaxDays = 30 },
            new AgingBucketDto { Label = "31-60 days", MinDays = 31, MaxDays = 60 },
            new AgingBucketDto { Label = "61-90 days", MinDays = 61, MaxDays = 90 },
            new AgingBucketDto { Label = "91-120 days", MinDays = 91, MaxDays = 120 },
            new AgingBucketDto { Label = "121-180 days", MinDays = 121, MaxDays = 180 },
            new AgingBucketDto { Label = ">180 days", MinDays = 181, MaxDays = int.MaxValue }
        };

        foreach (var invoice in invoices)
        {
            var days = invoice.DaysOverdue;
            var bucket = buckets.First(b => days >= b.MinDays && days <= b.MaxDays);
            bucket.Amount += invoice.RemainingAmount;
            bucket.InvoiceCount++;
        }

        return buckets.ToList();
    }
}
