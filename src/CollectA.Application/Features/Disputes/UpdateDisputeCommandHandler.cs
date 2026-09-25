using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Disputes;

public class UpdateDisputeCommandHandler : IRequestHandler<UpdateDisputeCommand, DisputeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public UpdateDisputeCommandHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<DisputeDto> Handle(UpdateDisputeCommand request, CancellationToken cancellationToken)
    {
        var dispute = await _context.Disputes
            .ForTenant(_tenantContext)
            .Include(d => d.Customer)
            .Include(d => d.Invoice)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (dispute == null) throw new KeyNotFoundException($"Dispute '{request.Id}' not found.");

        if (!DisputeWorkflow.CanTransitionTo(dispute.Status, request.Status))
        {
            throw new InvalidOperationException($"Cannot transition dispute from '{dispute.Status}' to '{request.Status}'.");
        }

        dispute.Title = request.Title;
        dispute.Description = request.Description;
        dispute.Type = request.Type;
        dispute.DisputedAmount = request.DisputedAmount;
        dispute.ResponsibleId = request.ResponsibleId;
        dispute.Department = request.Department;
        dispute.DueDate = request.DueDate;
        dispute.ResolutionNotes = request.ResolutionNotes;

        await ApplyStatusChangeAsync(dispute, request.Status, request.ResolvedAt, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        var responsibleName = dispute.ResponsibleId.HasValue
            ? await _userLookupService.GetUserFullNameAsync(dispute.ResponsibleId.Value, cancellationToken)
            : null;

        return GetDisputesQueryHandler.MapToDto(dispute, dispute.Customer.Name, dispute.Invoice?.InvoiceNumber, responsibleName);
    }

    private async Task ApplyStatusChangeAsync(Domain.Entities.Dispute dispute, DisputeStatus newStatus, DateTime? resolvedAt, CancellationToken cancellationToken)
    {
        var oldStatus = dispute.Status;
        if (oldStatus == newStatus) return;

        dispute.Status = newStatus;

        if (newStatus == DisputeStatus.Resolved)
        {
            dispute.ResolvedAt = resolvedAt ?? DateTime.UtcNow;
        }
        else if (!DisputeWorkflow.IsActive(newStatus))
        {
            dispute.ResolvedAt = resolvedAt;
        }

        if (dispute.InvoiceId.HasValue)
        {
            var invoice = await _context.Invoices
                .ForTenant(_tenantContext)
                .FirstOrDefaultAsync(i => i.Id == dispute.InvoiceId.Value, cancellationToken);

            if (invoice != null)
            {
                invoice.IsDisputed = DisputeWorkflow.IsActive(newStatus);
                if (!invoice.IsDisputed && !invoice.DisputedAt.HasValue)
                {
                    invoice.DisputedAt = null;
                }
                InvoiceStatusCalculator.Recalculate(invoice);
            }
        }
    }
}
