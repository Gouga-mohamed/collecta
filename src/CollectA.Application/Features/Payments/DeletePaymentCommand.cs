using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Payments;

public class DeletePaymentCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

public class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand, Unit>
{
    private readonly IIIApplicationDbContext _context;

    public DeletePaymentCommandHandler(IIIApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Include(p => p.Invoice)
            .Include(p => p.Cheque)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (payment == null) throw new KeyNotFoundException($"Payment '{request.Id}' not found.");

        if (payment.Invoice != null)
        {
            payment.Invoice.PaidAmount -= payment.Amount;
            if (payment.Invoice.PaidAmount < 0) payment.Invoice.PaidAmount = 0;
            UpdateInvoiceStatus.Recalculate(payment.Invoice);
        }

        if (payment.Cheque != null)
        {
            _context.Cheques.Remove(payment.Cheque);
        }

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
