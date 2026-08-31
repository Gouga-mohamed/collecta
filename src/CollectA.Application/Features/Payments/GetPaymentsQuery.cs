using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Models;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Payments;

public class GetPaymentsQuery : IRequest<PagedResult<PaymentDto>>
{
    public PaginationParams Pagination { get; set; } = new();
    public Guid? CustomerId { get; set; }
    public Guid? InvoiceId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, PagedResult<PaymentDto>>
{
    private readonly IIIApplicationDbContext _context;

    public GetPaymentsQueryHandler(IIIApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PaymentDto>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Payments
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Invoice)
            .Include(p => p.Cheque)
            .AsQueryable();

        if (request.CustomerId.HasValue)
        {
            query = query.Where(p => p.CustomerId == request.CustomerId.Value);
        }

        if (request.InvoiceId.HasValue)
        {
            query = query.Where(p => p.InvoiceId == request.InvoiceId.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate <= request.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var search = request.Pagination.Search.ToLowerInvariant();
            query = query.Where(p =>
                (p.Reference != null && p.Reference.ToLower().Contains(search)) ||
                p.Customer.Name.ToLower().Contains(search) ||
                (p.Invoice != null && p.Invoice.InvoiceNumber.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.Pagination.SortBy?.ToLowerInvariant() switch
        {
            "date" => request.Pagination.SortDescending ? query.OrderByDescending(p => p.PaymentDate) : query.OrderBy(p => p.PaymentDate),
            "amount" => request.Pagination.SortDescending ? query.OrderByDescending(p => p.Amount) : query.OrderBy(p => p.Amount),
            _ => query.OrderByDescending(p => p.PaymentDate)
        };

        var items = await query
            .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(p => MapToDto(p, p.Customer.Name, p.Invoice?.InvoiceNumber)).ToList();

        return new PagedResult<PaymentDto>
        {
            Items = dtos,
            PageNumber = request.Pagination.PageNumber,
            PageSize = request.Pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public static PaymentDto MapToDto(Domain.Entities.Payment payment, string customerName, string? invoiceNumber)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            CustomerId = payment.CustomerId,
            CustomerName = customerName,
            InvoiceId = payment.InvoiceId,
            InvoiceNumber = invoiceNumber,
            Amount = payment.Amount,
            Currency = payment.Currency,
            PaymentDate = payment.PaymentDate,
            Method = payment.Method,
            Reference = payment.Reference,
            BankName = payment.BankName,
            Notes = payment.Notes,
            Cheque = payment.Cheque == null ? null : new ChequeDto
            {
                Id = payment.Cheque.Id,
                ChequeNumber = payment.Cheque.ChequeNumber,
                BankName = payment.Cheque.BankName,
                Drawer = payment.Cheque.Drawer,
                DueDate = payment.Cheque.DueDate,
                Status = payment.Cheque.Status,
                Amount = payment.Cheque.Amount,
                Currency = payment.Cheque.Currency,
                Notes = payment.Cheque.Notes
            },
            CreatedAt = payment.CreatedAt
        };
    }
}
