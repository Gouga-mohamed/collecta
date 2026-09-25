using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Application.Common.Models;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.CollectionActions;

public class GetCollectionActionsQuery : IRequest<PagedResult<CollectionActionDto>>
{
    public PaginationParams Pagination { get; set; } = new();
    public Guid? CustomerId { get; set; }
    public Guid? InvoiceId { get; set; }
    public CollectionActionType? Type { get; set; }
    public CollectionActionOutcome? Outcome { get; set; }
    public Guid? AssignedToId { get; set; }
    public bool? IsClosed { get; set; }
}

public class GetCollectionActionsQueryHandler : IRequestHandler<GetCollectionActionsQuery, PagedResult<CollectionActionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public GetCollectionActionsQueryHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<PagedResult<CollectionActionDto>> Handle(GetCollectionActionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.CollectionActions
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(a => a.Customer)
            .Include(a => a.Invoice)
            .AsQueryable();

        if (request.CustomerId.HasValue)
        {
            query = query.Where(a => a.CustomerId == request.CustomerId.Value);
        }

        if (request.InvoiceId.HasValue)
        {
            query = query.Where(a => a.InvoiceId == request.InvoiceId.Value);
        }

        if (request.Type.HasValue)
        {
            query = query.Where(a => a.Type == request.Type.Value);
        }

        if (request.Outcome.HasValue)
        {
            query = query.Where(a => a.Outcome == request.Outcome.Value);
        }

        if (request.AssignedToId.HasValue)
        {
            query = query.Where(a => a.AssignedToId == request.AssignedToId.Value);
        }

        if (request.IsClosed.HasValue)
        {
            query = query.Where(a => a.IsClosed == request.IsClosed.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var search = request.Pagination.Search.ToLowerInvariant();
            query = query.Where(a =>
                a.Notes.ToLower().Contains(search) ||
                a.Customer.Name.ToLower().Contains(search) ||
                (a.Invoice != null && a.Invoice.InvoiceNumber.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.Pagination.SortBy?.ToLowerInvariant() switch
        {
            "actiondate" => request.Pagination.SortDescending ? query.OrderByDescending(a => a.ActionDate) : query.OrderBy(a => a.ActionDate),
            "type" => request.Pagination.SortDescending ? query.OrderByDescending(a => a.Type) : query.OrderBy(a => a.Type),
            "outcome" => request.Pagination.SortDescending ? query.OrderByDescending(a => a.Outcome) : query.OrderBy(a => a.Outcome),
            _ => query.OrderByDescending(a => a.ActionDate)
        };

        var items = await query
            .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<CollectionActionDto>();
        foreach (var item in items)
        {
            var assignedToName = item.AssignedToId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(item.AssignedToId.Value, cancellationToken)
                : null;
            var createdByName = await _userLookupService.GetUserFullNameAsync(item.CreatedById, cancellationToken);

            dtos.Add(MapToDto(item, item.Customer.Name, item.Invoice?.InvoiceNumber, assignedToName, createdByName));
        }

        return new PagedResult<CollectionActionDto>
        {
            Items = dtos,
            PageNumber = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public static CollectionActionDto MapToDto(Domain.Entities.CollectionAction action, string customerName, string? invoiceNumber, string? assignedToName, string? createdByName)
    {
        return new CollectionActionDto
        {
            Id = action.Id,
            CustomerId = action.CustomerId,
            CustomerName = customerName,
            InvoiceId = action.InvoiceId,
            InvoiceNumber = invoiceNumber,
            Type = action.Type,
            AssignedToId = action.AssignedToId,
            AssignedToName = assignedToName,
            CreatedByName = createdByName ?? "Unknown",
            ActionDate = action.ActionDate,
            DueDate = action.DueDate,
            Priority = action.Priority,
            Notes = action.Notes,
            Outcome = action.Outcome,
            OutcomeNotes = action.OutcomeNotes,
            IsClosed = action.IsClosed,
            ClosedAt = action.ClosedAt,
            CreatedAt = action.CreatedAt
        };
    }
}
