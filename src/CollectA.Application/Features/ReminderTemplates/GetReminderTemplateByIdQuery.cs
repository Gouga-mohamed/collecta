using CollectA.Domain.Common.Interfaces;
using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.ReminderTemplates;

public class GetReminderTemplateByIdQuery : IRequest<ReminderTemplateDto?>
{
    public Guid Id { get; set; }
}

public class GetReminderTemplateByIdQueryHandler : IRequestHandler<GetReminderTemplateByIdQuery, ReminderTemplateDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public GetReminderTemplateByIdQueryHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<ReminderTemplateDto?> Handle(GetReminderTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await _context.ReminderTemplates
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template == null) return null;

        return GetReminderTemplatesQueryHandler.MapToDto(template);
    }
}
