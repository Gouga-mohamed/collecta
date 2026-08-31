using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Application.Features.Receivables;
using CollectA.Domain.Enums;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Dashboard;

public class GetCfoDashboardQuery : IRequest<CfoDashboardDto>
{
}

public class GetCfoDashboardQueryHandler : IRequestHandler<GetCfoDashboardQuery, CfoDashboardDto>
{
    private readonly IIIApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public GetCfoDashboardQueryHandler(IIIApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<CfoDashboardDto> Handle(GetCfoDashboardQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var firstDayOfLastMonth = firstDayOfMonth.AddMonths(-1);
        var lastDayOfLastMonth = firstDayOfMonth.AddDays(-1);

        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .ForTenant(_tenantContext)
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.WrittenOff)
            .ToListAsync(cancellationToken);

        var totalOutstanding = invoices.Sum(i => i.RemainingAmount);
        var totalOverdue = invoices.Where(i => i.IsOverdue).Sum(i => i.RemainingAmount);
        var totalCurrent = totalOutstanding - totalOverdue;

        var openInvoicesCount = invoices.Count;
        var overdueInvoicesCount = invoices.Count(i => i.IsOverdue);
        var customersWithOverdue = invoices.Where(i => i.IsOverdue).Select(i => i.CustomerId).Distinct().Count();

        var activeCustomersCount = await _context.Customers
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .CountAsync(c => c.IsActive, cancellationToken);

        var paymentsThisMonth = await _context.Payments
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Where(p => p.PaymentDate >= firstDayOfMonth)
            .SumAsync(p => p.Amount, cancellationToken);

        var paymentsLastMonth = await _context.Payments
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Where(p => p.PaymentDate >= firstDayOfLastMonth && p.PaymentDate <= lastDayOfLastMonth)
            .SumAsync(p => p.Amount, cancellationToken);

        var invoicedThisMonth = await _context.Invoices
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Where(i => i.InvoiceDate >= firstDayOfMonth)
            .SumAsync(i => i.Amount, cancellationToken);

        var collectionRate = invoicedThisMonth > 0 ? paymentsThisMonth / invoicedThisMonth * 100 : 0;

        var dso = await CalculateDsoAsync(today, cancellationToken);

        var aging = await new GetAgingQueryHandler(_context).Handle(new GetAgingQuery(), cancellationToken);

        var topDebtors = invoices
            .GroupBy(i => new { i.CustomerId, i.Customer.Name })
            .Select(g => new TopDebtorDto
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.Name,
                TotalDue = g.Sum(i => i.RemainingAmount),
                TotalOverdue = g.Where(i => i.IsOverdue).Sum(i => i.RemainingAmount),
                RiskLevel = RiskLevel.Low
            })
            .OrderByDescending(d => d.TotalDue)
            .Take(10)
            .ToList();

        var monthlyCash = await GetMonthlyCashAsync(today, cancellationToken);

        var upcomingPromises = await _context.PromiseToPays
            .AsNoTracking()
            .Include(p => p.Customer)
            .ForTenant(_tenantContext)
            .Where(p => p.Status == PromiseStatus.Pending && p.PromiseDate >= today && p.PromiseDate <= today.AddDays(7))
            .OrderBy(p => p.PromiseDate)
            .Take(10)
            .Select(p => new UpcomingPromiseDto
            {
                PromiseId = p.Id,
                CustomerName = p.Customer.Name,
                PromisedAmount = p.PromisedAmount,
                PromiseDate = p.PromiseDate
            })
            .ToListAsync(cancellationToken);

        var recentActivities = await GetRecentActivitiesAsync(cancellationToken);

        return new CfoDashboardDto
        {
            TotalOutstanding = totalOutstanding,
            TotalOverdue = totalOverdue,
            TotalCurrent = totalCurrent,
            TotalPaidThisMonth = paymentsThisMonth,
            TotalPaidLastMonth = paymentsLastMonth,
            OverduePercentage = totalOutstanding > 0 ? totalOverdue / totalOutstanding * 100 : 0,
            CollectionRate = collectionRate,
            Dso = dso,
            OpenInvoicesCount = openInvoicesCount,
            OverdueInvoicesCount = overdueInvoicesCount,
            ActiveCustomersCount = activeCustomersCount,
            CustomersWithOverdueCount = customersWithOverdue,
            AgingBuckets = aging,
            TopDebtors = topDebtors,
            MonthlyCash = monthlyCash,
            UpcomingPromises = upcomingPromises,
            RecentActivities = recentActivities
        };
    }

    private async Task<decimal> CalculateDsoAsync(DateTime today, CancellationToken cancellationToken)
    {
        var periodDays = 90;
        var startDate = today.AddDays(-periodDays);

        var totalOutstanding = await _context.Invoices
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.WrittenOff)
            .SumAsync(i => i.RemainingAmount, cancellationToken);

        var totalInvoicedInPeriod = await _context.Invoices
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= today)
            .SumAsync(i => i.Amount, cancellationToken);

        var averageDailySales = totalInvoicedInPeriod / periodDays;

        return averageDailySales > 0 ? totalOutstanding / averageDailySales : 0;
    }

    private async Task<List<MonthlyCashDto>> GetMonthlyCashAsync(DateTime today, CancellationToken cancellationToken)
    {
        var months = Enumerable.Range(0, 12)
            .Select(i => today.AddMonths(-i))
            .Select(d => new DateTime(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc))
            .Reverse()
            .ToList();

        var result = new List<MonthlyCashDto>();

        foreach (var monthStart in months)
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

            result.Add(new MonthlyCashDto
            {
                Month = monthStart.ToString("yyyy-MM"),
                Invoiced = invoiced,
                Collected = collected
            });
        }

        return result;
    }

    private async Task<List<RecentActivityDto>> GetRecentActivitiesAsync(CancellationToken cancellationToken)
    {
        var actions = await _context.CollectionActions
            .AsNoTracking()
            .Include(a => a.Customer)
            .ForTenant(_tenantContext)
            .OrderByDescending(a => a.CreatedAt)
            .Take(20)
            .Select(a => new RecentActivityDto
            {
                Type = "Action",
                Description = $"{a.Type} - {a.Notes}",
                Date = a.CreatedAt,
                CustomerName = a.Customer.Name
            })
            .ToListAsync(cancellationToken);

        var promises = await _context.PromiseToPays
            .AsNoTracking()
            .Include(p => p.Customer)
            .ForTenant(_tenantContext)
            .OrderByDescending(p => p.CreatedAt)
            .Take(20)
            .Select(p => new RecentActivityDto
            {
                Type = "Promise",
                Description = $"Promise to pay {p.PromisedAmount:C} on {p.PromiseDate:yyyy-MM-dd}",
                Date = p.CreatedAt,
                CustomerName = p.Customer.Name
            })
            .ToListAsync(cancellationToken);

        var payments = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Customer)
            .ForTenant(_tenantContext)
            .OrderByDescending(p => p.CreatedAt)
            .Take(20)
            .Select(p => new RecentActivityDto
            {
                Type = "Payment",
                Description = $"Payment received {p.Amount:C}",
                Date = p.CreatedAt,
                CustomerName = p.Customer.Name
            })
            .ToListAsync(cancellationToken);

        var disputes = await _context.Disputes
            .AsNoTracking()
            .Include(d => d.Customer)
            .ForTenant(_tenantContext)
            .OrderByDescending(d => d.CreatedAt)
            .Take(20)
            .Select(d => new RecentActivityDto
            {
                Type = "Dispute",
                Description = $"Dispute: {d.Title}",
                Date = d.CreatedAt,
                CustomerName = d.Customer.Name
            })
            .ToListAsync(cancellationToken);

        return actions
            .Concat(promises)
            .Concat(payments)
            .Concat(disputes)
            .OrderByDescending(a => a.Date)
            .Take(20)
            .ToList();
    }
}
