using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Application.Common.Models;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.ReminderTemplates;

public class GetReminderTemplatesQuery : IRequest<PagedResult<ReminderTemplateDto>>
{
    public PaginationParams Pagination { get; set; } = new();
    public bool? IsActive { get; set; }
}

public class GetReminderTemplatesQueryHandler : IRequestHandler<GetReminderTemplatesQuery, PagedResult<ReminderTemplateDto>>
{
    private readonly IIIApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public GetReminderTemplatesQueryHandler(IIIApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<PagedResult<ReminderTemplateDto>> Handle(GetReminderTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ReminderTemplates
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(t => t.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var search = request.Pagination.Search.ToLowerInvariant();
            query = query.Where(t =>
                t.Name.ToLower().Contains(search) ||
                (t.Subject != null && t.Subject.ToLower().Contains(search)) ||
                (t.Channel != null && t.Channel.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.Pagination.SortBy?.ToLowerInvariant() switch
        {
            "name" => request.Pagination.SortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
            "created" => request.Pagination.SortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            _ => query.OrderBy(t => t.Name)
        };

        var items = await query
            .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(MapToDto).ToList();

        return new PagedResult<ReminderTemplateDto>
        {
            Items = dtos,
            PageNumber = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public static ReminderTemplateDto MapToDto(Domain.Entities.ReminderTemplate template)
    {
        return new ReminderTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Subject = template.Subject,
            Body = template.Body,
            Channel = template.Channel,
            IsBuiltIn = false
        };
    }
}
