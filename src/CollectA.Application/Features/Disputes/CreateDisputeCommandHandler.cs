using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Entities;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Disputes;

public class CreateDisputeCommandHandler : IRequestHandler<CreateDisputeCommand, DisputeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public CreateDisputeCommandHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<DisputeDto> Handle(CreateDisputeCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.ForTenant(_tenantContext).FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);
        if (customer == null) throw new KeyNotFoundException($"Customer '{request.CustomerId}' not found.");

        Invoice? invoice = null;
        if (request.InvoiceId.HasValue)
        {
            invoice = await _context.Invoices
                .ForTenant(_tenantContext)
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.Id == request.InvoiceId.Value && i.CustomerId == request.CustomerId, cancellationToken);
            if (invoice == null) throw new KeyNotFoundException($"Invoice '{request.InvoiceId}' not found for this customer.");
        }

        var dispute = new Dispute
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            CustomerId = request.CustomerId,
            InvoiceId = request.InvoiceId,
            Type = request.Type,
            Status = DisputeStatus.Open,
            DisputedAmount = request.DisputedAmount,
            Currency = request.Currency,
            ResponsibleId = request.ResponsibleId,
            Department = request.Department,
            DueDate = request.DueDate,
            ResolutionNotes = request.Notes
        };

        _context.Disputes.Add(dispute);

        if (invoice != null)
        {
            invoice.IsDisputed = true;
            invoice.DisputedAt = DateTime.UtcNow;
            InvoiceStatusCalculator.Recalculate(invoice);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var responsibleName = dispute.ResponsibleId.HasValue
            ? await _userLookupService.GetUserFullNameAsync(dispute.ResponsibleId.Value, cancellationToken)
            : null;

        return GetDisputesQueryHandler.MapToDto(dispute, customer.Name, invoice?.InvoiceNumber, responsibleName);
    }
}
