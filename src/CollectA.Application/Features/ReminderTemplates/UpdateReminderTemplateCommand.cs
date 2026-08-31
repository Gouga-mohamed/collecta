using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.ReminderTemplates;

public class UpdateReminderTemplateCommand : IRequest<ReminderTemplateDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Channel { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateReminderTemplateCommandHandler : IRequestHandler<UpdateReminderTemplateCommand, ReminderTemplateDto>
{
    private readonly IIIApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public UpdateReminderTemplateCommandHandler(IIIApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<ReminderTemplateDto> Handle(UpdateReminderTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.ReminderTemplates
            .ForTenant(_tenantContext)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template == null) throw new KeyNotFoundException($"Reminder template '{request.Id}' not found.");

        if (await _context.ReminderTemplates.AnyAsync(t => t.Name == request.Name && t.Id != request.Id, cancellationToken))
        {
            throw new InvalidOperationException($"Reminder template name '{request.Name}' already exists.");
        }

        template.Name = request.Name;
        template.Description = request.Description;
        template.Subject = request.Subject;
        template.Body = request.Body;
        template.Channel = request.Channel;
        template.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return GetReminderTemplatesQuery.MapToDto(template);
    }
}
