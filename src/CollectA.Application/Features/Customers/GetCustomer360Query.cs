using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Customers;

public class GetCustomer360Query : IRequest<Customer360Dto?>
{
    public Guid Id { get; set; }
}

public class GetCustomer360QueryHandler : IRequestHandler<GetCustomer360Query, Customer360Dto?>
{
    private readonly IIIApplicationDbContext _context;

    public GetCustomer360QueryHandler(IIIApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Customer360Dto?> Handle(GetCustomer360Query request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Include(c => c.Invoices)
            .Include(c => c.Payments)
            .Include(c => c.CollectionActions)
            .Include(c => c.Promises)
            .Include(c => c.Disputes)
            .Include(c => c.Contacts)
            .Include(c => c.RiskScores.OrderByDescending(r => r.CalculatedAt).Take(1))
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null) return null;

        var detail = await new GetCustomerByIdQueryHandler(_context).Handle(new GetCustomerByIdQuery { Id = request.Id }, cancellationToken);

        var totalInvoiced = customer.Invoices.Sum(i => i.Amount);
        var totalPaid = customer.Invoices.Sum(i => i.PaidAmount);
        var averageDailySales = totalInvoiced / 365m;
        var dso = averageDailySales > 0 ? (int)(customer.Invoices.Sum(i => i.RemainingAmount) / averageDailySales) : 0;

        var openInvoices = customer.Invoices.Where(i => i.Status != Domain.Enums.InvoiceStatus.Paid && i.Status != Domain.Enums.InvoiceStatus.Cancelled && i.Status != Domain.Enums.InvoiceStatus.WrittenOff);

        return new Customer360Dto
        {
            Customer = detail!,
            OpenInvoicesCount = openInvoices.Count(),
            OverdueInvoicesCount = openInvoices.Count(i => i.IsOverdue),
            DsoApproximate = dso,
            Invoices = customer.Invoices.OrderByDescending(i => i.InvoiceDate)
                .Select(i => GetInvoicesQuery.MapToDto(i, customer.Name)).ToList(),
            Payments = customer.Payments.OrderByDescending(p => p.PaymentDate)
                .Select(p => GetPaymentsQuery.MapToDto(p, customer.Name, null)).ToList(),
            Actions = customer.CollectionActions.OrderByDescending(a => a.ActionDate)
                .Select(a => GetCollectionActionsQuery.MapToDto(a, customer.Name)).ToList(),
            Promises = customer.Promises.OrderByDescending(p => p.PromiseDate)
                .Select(p => GetPromisesQuery.MapToDto(p, customer.Name)).ToList(),
            Disputes = customer.Disputes.OrderByDescending(d => d.CreatedAt)
                .Select(d => GetDisputesQuery.MapToDto(d, customer.Name)).ToList(),
            Notes = new List<NoteDto>()
        };
    }
}
