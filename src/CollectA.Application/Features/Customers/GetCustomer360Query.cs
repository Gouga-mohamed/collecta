using CollectA.Application.Features.Invoices;
using CollectA.Application.Features.Payments;
using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Domain.Common.Interfaces;
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
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public GetCustomer360QueryHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Customer360Dto?> Handle(GetCustomer360Query request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(c => c.Invoices)
            .Include(c => c.Payments)
            .Include(c => c.Contacts)
            .Include(c => c.RiskScores.OrderByDescending(r => r.CalculatedAt).Take(1))
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null) return null;

        var detail = await new GetCustomerByIdQueryHandler(_context, _tenantContext).Handle(new GetCustomerByIdQuery { Id = request.Id }, cancellationToken);

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
                .Select(i => GetInvoicesQueryHandler.MapToDto(i, customer.Name)).ToList(),
            Payments = customer.Payments.OrderByDescending(p => p.PaymentDate)
                .Select(p => GetPaymentsQueryHandler.MapToDto(p, customer.Name, null)).ToList(),
            Notes = new List<NoteDto>()
        };
    }
}
