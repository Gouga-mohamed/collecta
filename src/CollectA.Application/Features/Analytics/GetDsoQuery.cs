using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Enums;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Analytics;

public class GetDsoQuery : IRequest<DsoResultDto>
{
    public int PeriodDays { get; set; } = 90;
}

public class GetDsoQueryHandler : IRequestHandler<GetDsoQuery, DsoResultDto>
{
    private readonly IIIApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public GetDsoQueryHandler(IIIApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<DsoResultDto> Handle(GetDsoQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-request.PeriodDays);

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

        var averageDailySales = totalInvoicedInPeriod / request.PeriodDays;
        var dsoDays = averageDailySales > 0 ? (int)(totalOutstanding / averageDailySales) : 0;

        return new DsoResultDto
        {
            DsoDays = dsoDays,
            TotalOutstanding = totalOutstanding,
            AverageDailySales = averageDailySales,
            Period = $"Last {request.PeriodDays} days"
        };
    }
}
