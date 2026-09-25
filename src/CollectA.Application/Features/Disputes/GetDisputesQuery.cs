using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Application.Common.Models;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Disputes;

public class GetDisputesQuery : IRequest<PagedResult<DisputeDto>>
{
    public PaginationParams Pagination { get; set; } = new();
    public Guid? CustomerId { get; set; }
    public DisputeStatus? Status { get; set; }
    public DisputeType? Type { get; set; }
}

public class GetDisputesQueryHandler : IRequestHandler<GetDisputesQuery, PagedResult<DisputeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public GetDisputesQueryHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<PagedResult<DisputeDto>> Handle(GetDisputesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Disputes
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(d => d.Customer)
            .Include(d => d.Invoice)
            .AsQueryable();

        if (request.CustomerId.HasValue)
        {
            query = query.Where(d => d.CustomerId == request.CustomerId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(d => d.Status == request.Status.Value);
        }

        if (request.Type.HasValue)
        {
            query = query.Where(d => d.Type == request.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var search = request.Pagination.Search.ToLowerInvariant();
            query = query.Where(d =>
                d.Title.ToLower().Contains(search) ||
                d.Customer.Name.ToLower().Contains(search) ||
                (d.Invoice != null && d.Invoice.InvoiceNumber.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.Pagination.SortBy?.ToLowerInvariant() switch
        {
            "duedate" => request.Pagination.SortDescending ? query.OrderByDescending(d => d.DueDate) : query.OrderBy(d => d.DueDate),
            "status" => request.Pagination.SortDescending ? query.OrderByDescending(d => d.Status) : query.OrderBy(d => d.Status),
            "type" => request.Pagination.SortDescending ? query.OrderByDescending(d => d.Type) : query.OrderBy(d => d.Type),
            _ => query.OrderByDescending(d => d.CreatedAt)
        };

        var items = await query
            .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<DisputeDto>();
        foreach (var item in items)
        {
            var responsibleName = item.ResponsibleId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(item.ResponsibleId.Value, cancellationToken)
                : null;

            dtos.Add(MapToDto(item, item.Customer.Name, item.Invoice?.InvoiceNumber, responsibleName));
        }

        return new PagedResult<DisputeDto>
        {
            Items = dtos,
            PageNumber = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public static DisputeDto MapToDto(Domain.Entities.Dispute dispute, string customerName, string? invoiceNumber, string? responsibleName)
    {
        return new DisputeDto
        {
            Id = dispute.Id,
            Title = dispute.Title,
            Description = dispute.Description,
            CustomerId = dispute.CustomerId,
            CustomerName = customerName,
            InvoiceId = dispute.InvoiceId,
            InvoiceNumber = invoiceNumber,
            Type = dispute.Type,
            Status = dispute.Status,
            DisputedAmount = dispute.DisputedAmount,
            Currency = dispute.Currency,
            ResponsibleId = dispute.ResponsibleId,
            ResponsibleName = responsibleName,
            Department = dispute.Department,
            DueDate = dispute.DueDate,
            ResolvedAt = dispute.ResolvedAt,
            ResolutionNotes = dispute.ResolutionNotes,
            CreatedAt = dispute.CreatedAt
        };
    }
}
