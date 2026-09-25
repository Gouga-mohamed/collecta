using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common;
using CollectA.Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Disputes;

public class DeleteDisputeCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteDisputeCommandHandler : IRequestHandler<DeleteDisputeCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public DeleteDisputeCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task Handle(DeleteDisputeCommand request, CancellationToken cancellationToken)
    {
        var dispute = await _context.Disputes
            .ForTenant(_tenantContext)
            .Include(d => d.Invoice)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (dispute == null) throw new KeyNotFoundException($"Dispute '{request.Id}' not found.");

        var invoice = dispute.Invoice;
        var wasActive = DisputeWorkflow.IsActive(dispute.Status);

        _context.Disputes.Remove(dispute);

        if (invoice != null && wasActive)
        {
            var hasOtherActiveDisputes = await _context.Disputes
                .ForTenant(_tenantContext)
                .AnyAsync(d => d.Id != request.Id && d.InvoiceId == invoice.Id && DisputeWorkflow.IsActive(d.Status), cancellationToken);

            if (!hasOtherActiveDisputes)
            {
                invoice.IsDisputed = false;
                invoice.DisputedAt = null;
                InvoiceStatusCalculator.Recalculate(invoice);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
