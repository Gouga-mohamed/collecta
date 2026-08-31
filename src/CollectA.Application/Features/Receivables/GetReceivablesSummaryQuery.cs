using CollectA.Application.Common.Dtos;
using CollectA.Domain.Enums;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Receivables;

public class GetReceivablesSummaryQuery : IRequest<ReceivablesSummaryDto>
{
}

public class GetReceivablesSummaryQueryHandler : IRequestHandler<GetReceivablesSummaryQuery, ReceivablesSummaryDto>
{
    private readonly IIIApplicationDbContext _context;

    public GetReceivablesSummaryQueryHandler(IIIApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReceivablesSummaryDto> Handle(GetReceivablesSummaryQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.WrittenOff)
            .ToListAsync(cancellationToken);

        var totalOutstanding = invoices.Sum(i => i.RemainingAmount);
        var totalOverdue = invoices.Where(i => i.IsOverdue).Sum(i => i.RemainingAmount);
        var totalCurrent = totalOutstanding - totalOverdue;

        var today = DateTime.UtcNow.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var paymentsThisMonth = await _context.Payments
            .AsNoTracking()
            .Where(p => p.PaymentDate >= firstDayOfMonth)
            .SumAsync(p => p.Amount, cancellationToken);

        var customersWithOverdue = invoices.Where(i => i.IsOverdue).Select(i => i.CustomerId).Distinct().Count();

        var aging = await new GetAgingQueryHandler(_context).Handle(new GetAgingQuery(), cancellationToken);

        return new ReceivablesSummaryDto
        {
            TotalOutstanding = totalOutstanding,
            TotalOverdue = totalOverdue,
            TotalCurrent = totalCurrent,
            TotalPaidThisMonth = paymentsThisMonth,
            OverduePercentage = totalOutstanding > 0 ? totalOverdue / totalOutstanding * 100 : 0,
            OpenInvoicesCount = invoices.Count,
            OverdueInvoicesCount = invoices.Count(i => i.IsOverdue),
            CustomersWithOverdueCount = customersWithOverdue,
            AgingBuckets = aging
        };
    }
}
