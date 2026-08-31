using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Payments;

public class GetPaymentByIdQuery : IRequest<PaymentDto?>
{
    public Guid Id { get; set; }
}

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
{
    private readonly IIIApplicationDbContext _context;

    public GetPaymentByIdQueryHandler(IIIApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Invoice)
            .Include(p => p.Cheque)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (payment == null) return null;

        return GetPaymentsQuery.MapToDto(payment, payment.Customer.Name, payment.Invoice?.InvoiceNumber);
    }
}
