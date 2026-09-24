using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Domain.Common.Interfaces;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Receivables;

public class GetOverdueInvoicesQuery : IRequest<List<OverdueInvoiceDto>>
{
    public int? MinDays { get; set; }
    public int? MaxDays { get; set; }
    public Guid? CustomerId { get; set; }
    public int Take { get; set; } = 100;
}

public class GetOverdueInvoicesQueryHandler : IRequestHandler<GetOverdueInvoicesQuery, List<OverdueInvoiceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public GetOverdueInvoicesQueryHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<List<OverdueInvoiceDto>> Handle(GetOverdueInvoicesQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        var query = _context.Invoices
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(i => i.Customer)
            .Where(i => i.DueDate < today && (i.Amount - i.PaidAmount) > 0)
            .AsQueryable();

        if (request.CustomerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == request.CustomerId.Value);
        }

        if (request.MinDays.HasValue)
        {
            query = query.Where(i => i.DueDate <= today.AddDays(-request.MinDays.Value));
        }

        if (request.MaxDays.HasValue)
        {
            query = query.Where(i => i.DueDate > today.AddDays(-(request.MaxDays.Value + 1)));
        }

        var items = await query
            .OrderBy(i => i.DueDate)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        return items.Select(i => new OverdueInvoiceDto
        {
            InvoiceId = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            CustomerId = i.CustomerId,
            CustomerName = i.Customer.Name,
            DueDate = i.DueDate,
            Amount = i.Amount,
            RemainingAmount = i.RemainingAmount,
            DaysOverdue = i.DaysOverdue,
            Currency = i.Currency
        }).ToList();
    }
}
