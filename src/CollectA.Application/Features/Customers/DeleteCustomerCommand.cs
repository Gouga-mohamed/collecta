using CollectA.Application.Common.Extensions;
using CollectA.Domain.Common.Interfaces;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Customers;

public class DeleteCustomerCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public DeleteCustomerCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Unit> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .ForTenant(_tenantContext)
            .Include(c => c.Invoices)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null) throw new KeyNotFoundException($"Customer '{request.Id}' not found.");

        if (customer.Invoices.Any(i => i.Status != Domain.Enums.InvoiceStatus.Paid && i.Status != Domain.Enums.InvoiceStatus.Cancelled && i.Status != Domain.Enums.InvoiceStatus.WrittenOff))
        {
            throw new InvalidOperationException("Cannot delete a customer with open invoices.");
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
