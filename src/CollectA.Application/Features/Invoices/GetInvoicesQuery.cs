using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Models;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Enums;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Invoices;

public class GetInvoicesQuery : IRequest<PagedResult<InvoiceDto>>
{
    public PaginationParams Pagination { get; set; } = new();
    public Guid? CustomerId { get; set; }
    public InvoiceStatus? Status { get; set; }
    public bool? IsOverdue { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, PagedResult<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public GetInvoicesQueryHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<PagedResult<InvoiceDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(i => i.Customer)
            .AsQueryable();

        if (request.CustomerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == request.CustomerId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(i => i.Status == request.Status.Value);
        }

        if (request.IsOverdue.HasValue)
        {
            var today = DateTime.UtcNow.Date;
            query = request.IsOverdue.Value
                ? query.Where(i => i.DueDate < today && (i.Amount - i.PaidAmount) > 0)
                : query.Where(i => !(i.DueDate < today && (i.Amount - i.PaidAmount) > 0));
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(i => i.InvoiceDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(i => i.InvoiceDate <= request.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var search = request.Pagination.Search.ToLowerInvariant();
            query = query.Where(i =>
                i.InvoiceNumber.ToLower().Contains(search) ||
                i.Customer.Name.ToLower().Contains(search) ||
                (i.Description != null && i.Description.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.Pagination.SortBy?.ToLowerInvariant() switch
        {
            "invoicedate" => request.Pagination.SortDescending ? query.OrderByDescending(i => i.InvoiceDate) : query.OrderBy(i => i.InvoiceDate),
            "duedate" => request.Pagination.SortDescending ? query.OrderByDescending(i => i.DueDate) : query.OrderBy(i => i.DueDate),
            "amount" => request.Pagination.SortDescending ? query.OrderByDescending(i => i.Amount) : query.OrderBy(i => i.Amount),
            "status" => request.Pagination.SortDescending ? query.OrderByDescending(i => i.Status) : query.OrderBy(i => i.Status),
            _ => query.OrderByDescending(i => i.InvoiceDate)
        };

        var items = await query
            .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(i => MapToDto(i, i.Customer.Name)).ToList();

        return new PagedResult<InvoiceDto>
        {
            Items = dtos,
            PageNumber = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public static InvoiceDto MapToDto(Domain.Entities.Invoice invoice, string customerName)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            CustomerId = invoice.CustomerId,
            CustomerName = customerName,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Amount = invoice.Amount,
            PaidAmount = invoice.PaidAmount,
            Currency = invoice.Currency,
            Status = invoice.Status,
            Description = invoice.Description,
            PaymentTermsDays = invoice.PaymentTermsDays,
            IsDisputed = invoice.IsDisputed,
            DaysOverdue = invoice.DaysOverdue,
            IsOverdue = invoice.IsOverdue,
            CreatedAt = invoice.CreatedAt
        };
    }
}
