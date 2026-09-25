using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Models;
using CollectA.Application.Features.CollectionActions;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollectA.Api.Controllers;

[ApiController]
[Route("api/collection-actions")]
[Authorize]
public class CollectionActionsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IAuditService _auditService;

    public CollectionActionsController(ISender sender, IAuditService auditService)
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
    public async Task<ActionResult<PagedResult<CollectionActionDto>>> GetCollectionActions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? invoiceId = null,
        [FromQuery] CollectionActionType? type = null,
        [FromQuery] CollectionActionOutcome? outcome = null,
        [FromQuery] Guid? assignedToId = null,
        [FromQuery] bool? isClosed = null,
        CancellationToken cancellationToken = default)
    {
        GetTenantId();

        var query = new GetCollectionActionsQuery
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
            InvoiceId = invoiceId,
            Type = type,
            Outcome = outcome,
            AssignedToId = assignedToId,
            IsClosed = isClosed
        };

        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CollectionActionDto>> GetCollectionAction(Guid id, CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(new GetCollectionActionByIdQuery { Id = id }, cancellationToken);
        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CollectionActionDto>> CreateCollectionAction(CreateCollectionActionCommand command, CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(command, cancellationToken);
        await _auditService.LogAsync("Create", nameof(Domain.Entities.CollectionAction), result.Id, null, null, cancellationToken);

        return CreatedAtAction(nameof(GetCollectionAction), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CollectionActionDto>> UpdateCollectionAction(Guid id, UpdateCollectionActionCommand command, CancellationToken cancellationToken)
    {
        GetTenantId();

        command.Id = id;
        var result = await _sender.Send(command, cancellationToken);
        await _auditService.LogAsync("Update", nameof(Domain.Entities.CollectionAction), id, null, null, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCollectionAction(Guid id, CancellationToken cancellationToken)
    {
        GetTenantId();

        await _sender.Send(new DeleteCollectionActionCommand { Id = id }, cancellationToken);
        await _auditService.LogAsync("Delete", nameof(Domain.Entities.CollectionAction), id, null, null, cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<CollectionActionDto>> CloseCollectionAction(Guid id, CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(new CloseCollectionActionCommand { Id = id }, cancellationToken);
        await _auditService.LogAsync("Close", nameof(Domain.Entities.CollectionAction), id, null, null, cancellationToken);

        return Ok(result);
    }
}
