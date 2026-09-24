using CollectA.Application.Common.Extensions;
using CollectA.Domain.Common.Interfaces;
using CollectA.Application.Common.Dtos;
using CollectA.Domain.Entities;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.ReminderTemplates;

public class CreateReminderTemplateCommand : IRequest<ReminderTemplateDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Channel { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateReminderTemplateCommandHandler : IRequestHandler<CreateReminderTemplateCommand, ReminderTemplateDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateReminderTemplateCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<ReminderTemplateDto> Handle(CreateReminderTemplateCommand request, CancellationToken cancellationToken)
    {
        if (await _context.ReminderTemplates.ForTenant(_tenantContext).AnyAsync(t => t.Name == request.Name, cancellationToken))
        {
            throw new InvalidOperationException($"Reminder template name '{request.Name}' already exists.");
        }

        var template = new ReminderTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Subject = request.Subject,
            Body = request.Body,
            Channel = request.Channel,
            IsActive = request.IsActive
        };

        _context.ReminderTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        return GetReminderTemplatesQueryHandler.MapToDto(template);
    }
}
