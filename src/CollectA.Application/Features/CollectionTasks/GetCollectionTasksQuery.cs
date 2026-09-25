using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Application.Common.Models;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskStatus = CollectA.Domain.Enums.TaskStatus;

namespace CollectA.Application.Features.CollectionTasks;

public class GetCollectionTasksQuery : IRequest<PagedResult<CollectionTaskDto>>
{
    public PaginationParams Pagination { get; set; } = new();
    public Guid? CustomerId { get; set; }
    public Guid? AssignedToId { get; set; }
    public TaskStatus? Status { get; set; }
    public Priority? Priority { get; set; }
    public DateTime? DueBefore { get; set; }
}

public class GetCollectionTasksQueryHandler : IRequestHandler<GetCollectionTasksQuery, PagedResult<CollectionTaskDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public GetCollectionTasksQueryHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<PagedResult<CollectionTaskDto>> Handle(GetCollectionTasksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.CollectionTasks
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(t => t.Customer)
            .Include(t => t.Invoice)
            .AsQueryable();

        if (request.CustomerId.HasValue)
        {
            query = query.Where(t => t.CustomerId == request.CustomerId.Value);
        }

        if (request.AssignedToId.HasValue)
        {
            query = query.Where(t => t.AssignedToId == request.AssignedToId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }

        if (request.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == request.Priority.Value);
        }

        if (request.DueBefore.HasValue)
        {
            query = query.Where(t => t.DueDate <= request.DueBefore.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var search = request.Pagination.Search.ToLowerInvariant();
            query = query.Where(t =>
                t.Title.ToLower().Contains(search) ||
                t.Customer.Name.ToLower().Contains(search) ||
                (t.Invoice != null && t.Invoice.InvoiceNumber.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.Pagination.SortBy?.ToLowerInvariant() switch
        {
            "duedate" => request.Pagination.SortDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            "priority" => request.Pagination.SortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "status" => request.Pagination.SortDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        var items = await query
            .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<CollectionTaskDto>();
        foreach (var item in items)
        {
            var assignedToName = item.AssignedToId.HasValue
                ? await _userLookupService.GetUserFullNameAsync(item.AssignedToId.Value, cancellationToken)
                : null;

            dtos.Add(MapToDto(item, item.Customer.Name, item.Invoice?.InvoiceNumber, assignedToName));
        }

        return new PagedResult<CollectionTaskDto>
        {
            Items = dtos,
            PageNumber = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public static CollectionTaskDto MapToDto(Domain.Entities.CollectionTask task, string customerName, string? invoiceNumber, string? assignedToName)
    {
        return new CollectionTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            CustomerId = task.CustomerId,
            CustomerName = customerName,
            InvoiceId = task.InvoiceId,
            InvoiceNumber = invoiceNumber,
            AssignedToId = task.AssignedToId,
            AssignedToName = assignedToName,
            DueDate = task.DueDate,
            Status = task.Status,
            Priority = task.Priority,
            CompletedAt = task.CompletedAt,
            CompletedNotes = task.CompletedNotes,
            CreatedAt = task.CreatedAt
        };
    }
}
