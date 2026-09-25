using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Entities;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.CollectionActions;

public class CreateCollectionActionCommandHandler : IRequestHandler<CreateCollectionActionCommand, CollectionActionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public CreateCollectionActionCommandHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<CollectionActionDto> Handle(CreateCollectionActionCommand request, CancellationToken cancellationToken)
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

        var action = new CollectionAction
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            InvoiceId = request.InvoiceId,
            Type = request.Type,
            AssignedToId = request.AssignedToId,
            CreatedById = _tenantContext.CurrentUserId ?? Guid.Empty,
            ActionDate = request.ActionDate,
            DueDate = request.DueDate,
            Priority = request.Priority,
            Notes = request.Notes,
            Outcome = CollectionActionOutcome.None,
            IsClosed = false
        };

        _context.CollectionActions.Add(action);
        await _context.SaveChangesAsync(cancellationToken);

        var assignedToName = action.AssignedToId.HasValue
            ? await _userLookupService.GetUserFullNameAsync(action.AssignedToId.Value, cancellationToken)
            : null;
        var createdByName = await _userLookupService.GetUserFullNameAsync(action.CreatedById, cancellationToken);

        return GetCollectionActionsQueryHandler.MapToDto(action, customer.Name, null, assignedToName, createdByName);
    }
}
