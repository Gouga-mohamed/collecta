using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Models;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Customers;

public class GetCustomersQuery : IRequest<PagedResult<CustomerDto>>
{
    public PaginationParams Pagination { get; set; } = new();
    public bool? IsActive { get; set; }
    public string? Industry { get; set; }
}

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    private readonly IIIApplicationDbContext _context;

    public GetCustomersQueryHandler(IIIApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Customers
            .AsNoTracking()
            .Include(c => c.Invoices)
            .Include(c => c.RiskScores.OrderByDescending(r => r.CalculatedAt).Take(1))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var search = request.Pagination.Search.ToLowerInvariant();
            query = query.Where(c =>
                c.Name.ToLower().Contains(search) ||
                c.Code.ToLower().Contains(search) ||
                (c.Email != null && c.Email.ToLower().Contains(search)) ||
                (c.City != null && c.City.ToLower().Contains(search)));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Industry))
        {
            query = query.Where(c => c.Industry == request.Industry);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.Pagination.SortBy?.ToLowerInvariant() switch
        {
            "name" => request.Pagination.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "code" => request.Pagination.SortDescending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
            "created" => request.Pagination.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderBy(c => c.Name)
        };

        var items = await query
            .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(c => MapToDto(c)).ToList();

        return new PagedResult<CustomerDto>
        {
            Items = dtos,
            PageNumber = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public static CustomerDto MapToDto(Domain.Entities.Customer c)
    {
        var totalInvoiced = c.Invoices.Sum(i => i.Amount);
        var totalPaid = c.Invoices.Sum(i => i.PaidAmount);
        var totalDue = c.Invoices.Where(i => i.Status != Domain.Enums.InvoiceStatus.Paid && i.Status != Domain.Enums.InvoiceStatus.Cancelled && i.Status != Domain.Enums.InvoiceStatus.WrittenOff).Sum(i => i.RemainingAmount);
        var totalOverdue = c.Invoices.Where(i => i.IsOverdue).Sum(i => i.RemainingAmount);
        var latestRisk = c.RiskScores.FirstOrDefault();

        return new CustomerDto
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            LegalName = c.LegalName,
            TaxId = c.TaxId,
            Industry = c.Industry,
            City = c.City,
            Country = c.Country,
            PhoneNumber = c.PhoneNumber,
            Email = c.Email,
            PaymentTermsDays = c.PaymentTermsDays,
            CreditLimit = c.CreditLimit,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            TotalInvoiced = totalInvoiced,
            TotalPaid = totalPaid,
            TotalDue = totalDue,
            TotalOverdue = totalOverdue,
            RiskLevel = latestRisk?.Level ?? Domain.Enums.RiskLevel.Low
        };
    }
}
