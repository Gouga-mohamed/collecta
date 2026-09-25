using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Entities;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Promises;

public class CreatePromiseToPayCommandHandler : IRequestHandler<CreatePromiseToPayCommand, PromiseToPayDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public CreatePromiseToPayCommandHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<PromiseToPayDto> Handle(CreatePromiseToPayCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.ForTenant(_tenantContext).FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);
        if (customer == null) throw new KeyNotFoundException($"Customer '{request.CustomerId}' not found.");

        if (request.InvoiceId.HasValue)
        {
            var invoiceExists = await _context.Invoices
                .ForTenant(_tenantContext)
                .AnyAsync(i => i.Id == request.InvoiceId.Value && i.CustomerId == request.CustomerId, cancellationToken);
            if (!invoiceExists) throw new KeyNotFoundException($"Invoice '{request.InvoiceId}' not found for this customer.");
        }

        var promise = new PromiseToPay
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            InvoiceId = request.InvoiceId,
            PromisedAmount = request.PromisedAmount,
            Currency = request.Currency,
            PromiseDate = request.PromiseDate,
            ResponsibleAgentId = request.ResponsibleAgentId,
            Status = PromiseStatus.Pending,
            Notes = request.Notes
        };

        _context.PromiseToPays.Add(promise);
        await _context.SaveChangesAsync(cancellationToken);

        var responsibleName = promise.ResponsibleAgentId.HasValue
            ? await _userLookupService.GetUserFullNameAsync(promise.ResponsibleAgentId.Value, cancellationToken)
            : null;

        return GetPromisesQueryHandler.MapToDto(promise, customer.Name, null, responsibleName);
    }
}
