using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Analytics;

public class GetCollectionsAnalyticsQuery : IRequest<CollectionsAnalyticsDto>
{
    public int TrendMonths { get; set; } = 12;
}

public class GetCollectionsAnalyticsQueryHandler : IRequestHandler<GetCollectionsAnalyticsQuery, CollectionsAnalyticsDto>
{
    private readonly IIIApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public GetCollectionsAnalyticsQueryHandler(IIIApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<CollectionsAnalyticsDto> Handle(GetCollectionsAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startDate = firstDayOfMonth.AddMonths(-request.TrendMonths + 1);

        var totalInvoiced = await _context.Invoices
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= today)
            .SumAsync(i => i.Amount, cancellationToken);

        var totalCollected = await _context.Payments
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= today)
            .SumAsync(p => p.Amount, cancellationToken);

        var collectionRate = totalInvoiced > 0 ? totalCollected / totalInvoiced * 100 : 0;

        var averagePaymentDelay = await CalculateAveragePaymentDelayAsync(startDate, today, cancellationToken);

        var trends = await GetTrendsAsync(today, request.TrendMonths, cancellationToken);

        var topPayers = await GetTopPayersAsync(startDate, today, cancellationToken);

        return new CollectionsAnalyticsDto
        {
            TotalCollected = totalCollected,
            TotalInvoiced = totalInvoiced,
            CollectionRate = collectionRate,
            AveragePaymentDelay = averagePaymentDelay,
            Trends = trends,
            TopPayers = topPayers
        };
    }

    private async Task<decimal> CalculateAveragePaymentDelayAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var paymentsWithInvoices = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Invoice)
            .ForTenant(_tenantContext)
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate && p.Invoice != null)
            .ToListAsync(cancellationToken);

        var delays = paymentsWithInvoices
            .Where(p => p.Invoice!.DueDate.Date < p.PaymentDate.Date)
            .Select(p => (p.PaymentDate.Date - p.Invoice!.DueDate.Date).Days)
            .ToList();

        return delays.Any() ? delays.Average(d => (decimal)d) : 0;
    }

    private async Task<List<CollectionTrendPointDto>> GetTrendsAsync(DateTime today, int months, CancellationToken cancellationToken)
    {
        var monthStarts = Enumerable.Range(0, months)
            .Select(i => today.AddMonths(-i))
            .Select(d => new DateTime(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc))
            .Reverse()
            .ToList();

        var result = new List<CollectionTrendPointDto>();

        foreach (var monthStart in monthStarts)
        {
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var invoiced = await _context.Invoices
                .AsNoTracking()
                .ForTenant(_tenantContext)
                .Where(i => i.InvoiceDate >= monthStart && i.InvoiceDate <= monthEnd)
                .SumAsync(i => i.Amount, cancellationToken);

            var collected = await _context.Payments
                .AsNoTracking()
                .ForTenant(_tenantContext)
                .Where(p => p.PaymentDate >= monthStart && p.PaymentDate <= monthEnd)
                .SumAsync(p => p.Amount, cancellationToken);

            result.Add(new CollectionTrendPointDto
            {
                Period = monthStart.ToString("yyyy-MM"),
                Invoiced = invoiced,
                Collected = collected
            });
        }

        return result;
    }

    private async Task<List<CustomerCollectionDto>> GetTopPayersAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var payments = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Customer)
            .ForTenant(_tenantContext)
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
            .ToListAsync(cancellationToken);

        var invoices = await _context.Invoices
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate)
            .ToListAsync(cancellationToken);

        var collectedByCustomer = payments
            .GroupBy(p => new { p.CustomerId, p.Customer.Name })
            .Select(g => new
            {
                g.Key.CustomerId,
                g.Key.Name,
                Collected = g.Sum(p => p.Amount)
            })
            .ToList();

        var invoicedByCustomer = invoices
            .GroupBy(i => i.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                Invoiced = g.Sum(i => i.Amount)
            })
            .ToList();

        var result = collectedByCustomer
            .Select(c => new CustomerCollectionDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.Name,
                Collected = c.Collected,
                Invoiced = invoicedByCustomer.FirstOrDefault(i => i.CustomerId == c.CustomerId)?.Invoiced ?? 0
            })
            .OrderByDescending(c => c.Collected)
            .Take(10)
            .ToList();

        return result;
    }
}
