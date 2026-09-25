using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Models;
using CollectA.Application.Features.Disputes;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollectA.Api.Controllers;

[ApiController]
[Route("api/disputes")]
[Authorize]
public class DisputesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IAuditService _auditService;

    public DisputesController(ISender sender, IAuditService auditService)
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
    public async Task<ActionResult<PagedResult<DisputeDto>>> GetDisputes(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] Guid? customerId = null,
        [FromQuery] DisputeStatus? status = null,
        [FromQuery] DisputeType? type = null,
        CancellationToken cancellationToken = default)
    {
        GetTenantId();

        var query = new GetDisputesQuery
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
            Status = status,
            Type = type
        };

        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DisputeDto>> GetDispute(Guid id, CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(new GetDisputeByIdQuery { Id = id }, cancellationToken);
        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DisputeDto>> CreateDispute(CreateDisputeCommand command, CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(command, cancellationToken);
        await _auditService.LogAsync("Create", nameof(Domain.Entities.Dispute), result.Id, null, null, cancellationToken);

        return CreatedAtAction(nameof(GetDispute), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DisputeDto>> UpdateDispute(Guid id, UpdateDisputeCommand command, CancellationToken cancellationToken)
    {
        GetTenantId();

        command.Id = id;
        var result = await _sender.Send(command, cancellationToken);
        await _auditService.LogAsync("Update", nameof(Domain.Entities.Dispute), id, null, null, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDispute(Guid id, CancellationToken cancellationToken)
    {
        GetTenantId();

        await _sender.Send(new DeleteDisputeCommand { Id = id }, cancellationToken);
        await _auditService.LogAsync("Delete", nameof(Domain.Entities.Dispute), id, null, null, cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<DisputeDto>> ChangeDisputeStatus(Guid id, ChangeDisputeStatusCommand command, CancellationToken cancellationToken)
    {
        GetTenantId();

        command.Id = id;
        var result = await _sender.Send(command, cancellationToken);
        await _auditService.LogAsync("ChangeStatus", nameof(Domain.Entities.Dispute), id, null, null, cancellationToken);

        return Ok(result);
    }
}
