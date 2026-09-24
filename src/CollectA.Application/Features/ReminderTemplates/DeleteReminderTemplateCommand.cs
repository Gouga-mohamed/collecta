using CollectA.Domain.Common.Interfaces;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.ReminderTemplates;

public class DeleteReminderTemplateCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

public class DeleteReminderTemplateCommandHandler : IRequestHandler<DeleteReminderTemplateCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public DeleteReminderTemplateCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Unit> Handle(DeleteReminderTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.ReminderTemplates
            .ForTenant(_tenantContext)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template == null) throw new KeyNotFoundException($"Reminder template '{request.Id}' not found.");

        _context.ReminderTemplates.Remove(template);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
