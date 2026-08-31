using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Invoices;

public class DeleteInvoiceCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

public class DeleteInvoiceCommandHandler : IRequestHandler<DeleteInvoiceCommand, Unit>
{
    private readonly IIIApplicationDbContext _context;

    public DeleteInvoiceCommandHandler(IIIApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (invoice == null) throw new KeyNotFoundException($"Invoice '{request.Id}' not found.");

        if (invoice.Payments.Any())
        {
            throw new InvalidOperationException("Cannot delete an invoice with payments.");
        }

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
