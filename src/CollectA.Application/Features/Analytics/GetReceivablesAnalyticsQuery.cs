using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Application.Features.Receivables;
using CollectA.Domain.Enums;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Analytics;

public class GetReceivablesAnalyticsQuery : IRequest<ReceivablesAnalyticsDto>
{
    public int TrendMonths { get; set; } = 12;
}

public class GetReceivablesAnalyticsQueryHandler : IRequestHandler<GetReceivablesAnalyticsQuery, ReceivablesAnalyticsDto>
{
    private readonly IIIApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public GetReceivablesAnalyticsQueryHandler(IIIApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<ReceivablesAnalyticsDto> Handle(GetReceivablesAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .ForTenant(_tenantContext)
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.WrittenOff)
            .ToListAsync(cancellationToken);

        var totalOutstanding = invoices.Sum(i => i.RemainingAmount);
        var totalOverdue = invoices.Where(i => i.IsOverdue).Sum(i => i.RemainingAmount);
        var totalCurrent = totalOutstanding - totalOverdue;

        var overdueInvoices = invoices.Where(i => i.IsOverdue).ToList();
        var averageDaysOverdue = overdueInvoices.Any()
            ? overdueInvoices.Average(i => i.DaysOverdue)
            : 0;

        var aging = await new GetAgingQueryHandler(_context).Handle(new GetAgingQuery(), cancellationToken);

        var trends = await GetTrendsAsync(today, request.TrendMonths, cancellationToken);

        var topDebtors = invoices
            .GroupBy(i => new { i.CustomerId, i.Customer.Name })
            .Select(g => new CustomerReceivableDto
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.Name,
                TotalDue = g.Sum(i => i.RemainingAmount),
                TotalOverdue = g.Where(i => i.IsOverdue).Sum(i => i.RemainingAmount)
            })
            .OrderByDescending(d => d.TotalDue)
            .Take(10)
            .ToList();

        return new ReceivablesAnalyticsDto
        {
            TotalOutstanding = totalOutstanding,
            TotalOverdue = totalOverdue,
            TotalCurrent = totalCurrent,
            OverduePercentage = totalOutstanding > 0 ? totalOverdue / totalOutstanding * 100 : 0,
            AverageDaysOverdue = averageDaysOverdue,
            OpenInvoicesCount = invoices.Count,
            OverdueInvoicesCount = overdueInvoices.Count,
            AgingBuckets = aging,
            Trends = trends,
            TopDebtors = topDebtors
        };
    }

    private async Task<List<ReceivablesTrendPointDto>> GetTrendsAsync(DateTime today, int months, CancellationToken cancellationToken)
    {
        var monthStarts = Enumerable.Range(0, months)
            .Select(i => today.AddMonths(-i))
            .Select(d => new DateTime(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc))
            .Reverse()
            .ToList();

        var result = new List<ReceivablesTrendPointDto>();

        foreach (var monthStart in monthStarts)
        {
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var snapshotOutstanding = await _context.Invoices
                .AsNoTracking()
                .ForTenant(_tenantContext)
                .Where(i => i.Status != InvoiceStatus.Paid
                            && i.Status != InvoiceStatus.Cancelled
                            && i.Status != InvoiceStatus.WrittenOff
                            && i.InvoiceDate <= monthEnd)
                .SumAsync(i => i.RemainingAmount, cancellationToken);

            var snapshotOverdue = await _context.Invoices
                .AsNoTracking()
                .ForTenant(_tenantContext)
                .Where(i => i.Status != InvoiceStatus.Paid
                            && i.Status != InvoiceStatus.Cancelled
                            && i.Status != InvoiceStatus.WrittenOff
                            && i.InvoiceDate <= monthEnd
                            && i.DueDate < monthStart.AddMonths(1)
                            && i.RemainingAmount > 0)
                .SumAsync(i => i.RemainingAmount, cancellationToken);

            result.Add(new ReceivablesTrendPointDto
            {
                Period = monthStart.ToString("yyyy-MM"),
                Outstanding = snapshotOutstanding,
                Overdue = snapshotOverdue
            });
        }

        return result;
    }
}
