using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Entities;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskStatus = CollectA.Domain.Enums.TaskStatus;

namespace CollectA.Application.Features.CollectionTasks;

public class CreateCollectionTaskCommandHandler : IRequestHandler<CreateCollectionTaskCommand, CollectionTaskDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IUserLookupService _userLookupService;

    public CreateCollectionTaskCommandHandler(IApplicationDbContext context, ITenantContext tenantContext, IUserLookupService userLookupService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _userLookupService = userLookupService;
    }

    public async Task<CollectionTaskDto> Handle(CreateCollectionTaskCommand request, CancellationToken cancellationToken)
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

        var task = new CollectionTask
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            CustomerId = request.CustomerId,
            InvoiceId = request.InvoiceId,
            AssignedToId = request.AssignedToId,
            DueDate = request.DueDate,
            Status = TaskStatus.Pending,
            Priority = request.Priority
        };

        _context.CollectionTasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        var assignedToName = task.AssignedToId.HasValue
            ? await _userLookupService.GetUserFullNameAsync(task.AssignedToId.Value, cancellationToken)
            : null;

        return GetCollectionTasksQueryHandler.MapToDto(task, customer.Name, null, assignedToName);
    }
}
