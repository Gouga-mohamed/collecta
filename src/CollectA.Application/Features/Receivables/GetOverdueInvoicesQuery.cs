using CollectA.Application.Common.Dtos;
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
    private readonly IIIApplicationDbContext _context;

    public GetOverdueInvoicesQueryHandler(IIIApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OverdueInvoiceDto>> Handle(GetOverdueInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Where(i => i.IsOverdue)
            .AsQueryable();

        if (request.CustomerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == request.CustomerId.Value);
        }

        if (request.MinDays.HasValue)
        {
            query = query.Where(i => i.DaysOverdue >= request.MinDays.Value);
        }

        if (request.MaxDays.HasValue)
        {
            query = query.Where(i => i.DaysOverdue <= request.MaxDays.Value);
        }

        var items = await query
            .OrderByDescending(i => i.DaysOverdue)
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
