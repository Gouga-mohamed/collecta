using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Application.Common.Models;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Promises;

public class GetPromisesQuery : IRequest<PagedResult<PromiseToPayDto>>
{
    public PaginationParams Pagination { get; set; } = new();
    public Guid? CustomerId { get; set; }
    public PromiseStatus? Status { get; set; }
    public DateTime? DueBefore { get; set; }
}

public class GetPromisesQueryHandler : IRequestHandler<GetPromisesQuery, PagedResult<PromiseToPayDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public GetPromisesQueryHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<PagedResult<PromiseToPayDto>> Handle(GetPromisesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PromiseToPays
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(p => p.Customer)
            .Include(p => p.Invoice)
            .AsQueryable();

        if (request.CustomerId.HasValue)
        {
            query = query.Where(p => p.CustomerId == request.CustomerId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }

        if (request.DueBefore.HasValue)
        {
            query = query.Where(p => p.PromiseDate <= request.DueBefore.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var search = request.Pagination.Search.ToLowerInvariant();
            query = query.Where(p =>
                p.Customer.Name.ToLower().Contains(search) ||
                (p.Invoice != null && p.Invoice.InvoiceNumber.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.Pagination.SortBy?.ToLowerInvariant() switch
        {
            "promisedate" => request.Pagination.SortDescending ? query.OrderByDescending(p => p.PromiseDate) : query.OrderBy(p => p.PromiseDate),
            "amount" => request.Pagination.SortDescending ? query.OrderByDescending(p => p.PromisedAmount) : query.OrderBy(p => p.PromisedAmount),
            "status" => request.Pagination.SortDescending ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
            _ => query.OrderByDescending(p => p.PromiseDate)
        };

        var items = await query
            .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<PromiseToPayDto>();
        foreach (var item in items)
        {
            var responsibleName = item.ResponsibleAgentId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(item.ResponsibleAgentId.Value, cancellationToken)
                : null;

            dtos.Add(MapToDto(item, item.Customer.Name, item.Invoice?.InvoiceNumber, responsibleName));
        }

        return new PagedResult<PromiseToPayDto>
        {
            Items = dtos,
            PageNumber = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public static PromiseToPayDto MapToDto(Domain.Entities.PromiseToPay promise, string customerName, string? invoiceNumber, string? responsibleAgentName)
    {
        return new PromiseToPayDto
        {
            Id = promise.Id,
            CustomerId = promise.CustomerId,
            CustomerName = customerName,
            InvoiceId = promise.InvoiceId,
            InvoiceNumber = invoiceNumber,
            PromisedAmount = promise.PromisedAmount,
            Currency = promise.Currency,
            PromiseDate = promise.PromiseDate,
            ResponsibleAgentId = promise.ResponsibleAgentId,
            ResponsibleAgentName = responsibleAgentName,
            Status = promise.Status,
            FulfilledAmount = promise.FulfilledAmount,
            FulfilledDate = promise.FulfilledDate,
            Notes = promise.Notes,
            CreatedAt = promise.CreatedAt
        };
    }
}
