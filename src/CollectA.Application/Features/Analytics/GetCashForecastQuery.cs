using CollectA.Domain.Common.Interfaces;
using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Analytics;

public class GetCashForecastQuery : IRequest<CashForecastDto>
{
    public int Periods { get; set; } = 6;
}

public class GetCashForecastQueryHandler : IRequestHandler<GetCashForecastQuery, CashForecastDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public GetCashForecastQueryHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<CashForecastDto> Handle(GetCashForecastQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var startOfCurrentWeek = today.AddDays(-(int)today.DayOfWeek);
        var collectionRate = await GetHistoricalCollectionRateAsync(today, cancellationToken);

        var contractual = new List<CashForecastPointDto>();
        var expected = new List<CashForecastPointDto>();

        for (int i = 0; i < request.Periods; i++)
        {
            var periodStart = startOfCurrentWeek.AddDays(i * 7);
            var periodEnd = periodStart.AddDays(6);
            var periodLabel = $"{periodStart:yyyy-MM-dd} - {periodEnd:yyyy-MM-dd}";

            var dueInPeriod = await _context.Invoices
                .AsNoTracking()
                .ForTenant(_tenantContext)
                .Where(inv => inv.Status != InvoiceStatus.Paid
                              && inv.Status != InvoiceStatus.Cancelled
                              && inv.Status != InvoiceStatus.WrittenOff
                              && inv.DueDate >= periodStart
                              && inv.DueDate <= periodEnd)
                .SumAsync(inv => inv.Amount - inv.PaidAmount, cancellationToken);

            var promisedInPeriod = await _context.PromiseToPays
                .AsNoTracking()
                .ForTenant(_tenantContext)
                .Where(p => p.Status == PromiseStatus.Pending
                            && p.PromiseDate >= periodStart
                            && p.PromiseDate <= periodEnd)
                .SumAsync(p => p.PromisedAmount, cancellationToken);

            contractual.Add(new CashForecastPointDto
            {
                Period = periodLabel,
                Amount = dueInPeriod
            });

            var expectedAmount = dueInPeriod * (collectionRate / 100);
            expectedAmount = Math.Max(expectedAmount, promisedInPeriod);

            expected.Add(new CashForecastPointDto
            {
                Period = periodLabel,
                Amount = expectedAmount
            });
        }

        return new CashForecastDto
        {
            Contractual = contractual,
            Expected = expected
        };
    }

    private async Task<decimal> GetHistoricalCollectionRateAsync(DateTime today, CancellationToken cancellationToken)
    {
        var startDate = today.AddDays(-90);

        var invoiced = await _context.Invoices
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= today)
            .SumAsync(i => i.Amount, cancellationToken);

        var collected = await _context.Payments
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= today)
            .SumAsync(p => p.Amount, cancellationToken);

        return invoiced > 0 ? collected / invoiced * 100 : 0;
    }
}
