using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Models;
using CollectA.Application.Features.CollectionTasks;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskStatus = CollectA.Domain.Enums.TaskStatus;

namespace CollectA.Api.Controllers;

[ApiController]
[Route("api/collection-tasks")]
[Authorize]
public class CollectionTasksController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IAuditService _auditService;

    public CollectionTasksController(ISender sender, IAuditService auditService)
    {
        _sender = sender;
        _auditService = auditService;
    }

    private Guid GetTenantId()
    {
        var claim = User.FindFirstValue("tenant_id");
        return Guid.Parse(claim!);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CollectionTaskDto>>> GetCollectionTasks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? assignedToId = null,
        [FromQuery] TaskStatus? status = null,
        [FromQuery] Priority? priority = null,
        [FromQuery] DateTime? dueBefore = null,
        CancellationToken cancellationToken = default)
    {
        GetTenantId();

        var query = new GetCollectionTasksQuery
        {
            Pagination = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                SortBy = sortBy,
                SortDescending = sortDescending
            },
            CustomerId = customerId,
            AssignedToId = assignedToId,
            Status = status,
            Priority = priority,
            DueBefore = dueBefore
        };

        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CollectionTaskDto>> GetCollectionTask(Guid id, CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(new GetCollectionTaskByIdQuery { Id = id }, cancellationToken);
        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CollectionTaskDto>> CreateCollectionTask(CreateCollectionTaskCommand command, CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(command, cancellationToken);
        await _auditService.LogAsync("Create", nameof(Domain.Entities.CollectionTask), result.Id, null, null, cancellationToken);

        return CreatedAtAction(nameof(GetCollectionTask), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CollectionTaskDto>> UpdateCollectionTask(Guid id, UpdateCollectionTaskCommand command, CancellationToken cancellationToken)
    {
        GetTenantId();

        command.Id = id;
        var result = await _sender.Send(command, cancellationToken);
        await _auditService.LogAsync("Update", nameof(Domain.Entities.CollectionTask), id, null, null, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCollectionTask(Guid id, CancellationToken cancellationToken)
    {
        GetTenantId();

        await _sender.Send(new DeleteCollectionTaskCommand { Id = id }, cancellationToken);
        await _auditService.LogAsync("Delete", nameof(Domain.Entities.CollectionTask), id, null, null, cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<CollectionTaskDto>> CompleteCollectionTask(Guid id, [FromBody] CompleteTaskCommand command, CancellationToken cancellationToken)
    {
        GetTenantId();

        command.Id = id;
        var result = await _sender.Send(command, cancellationToken);
        await _auditService.LogAsync("Complete", nameof(Domain.Entities.CollectionTask), id, null, null, cancellationToken);

        return Ok(result);
    }
}
