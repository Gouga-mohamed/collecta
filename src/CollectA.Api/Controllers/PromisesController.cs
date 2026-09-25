using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Models;
using CollectA.Application.Features.Promises;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollectA.Api.Controllers;

[ApiController]
[Route("api/promises")]
[Authorize]
public class PromisesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IAuditService _auditService;

    public PromisesController(ISender sender, IAuditService auditService)
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
    public async Task<ActionResult<PagedResult<PromiseToPayDto>>> GetPromises(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] Guid? customerId = null,
        [FromQuery] PromiseStatus? status = null,
        [FromQuery] DateTime? dueBefore = null,
        CancellationToken cancellationToken = default)
    {
        GetTenantId();

        var query = new GetPromisesQuery
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
            DueBefore = dueBefore
        };

        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PromiseToPayDto>> GetPromise(Guid id, CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(new GetPromiseToPayByIdQuery { Id = id }, cancellationToken);
        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PromiseToPayDto>> CreatePromise(CreatePromiseToPayCommand command, CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(command, cancellationToken);
        await _auditService.LogAsync("Create", nameof(Domain.Entities.PromiseToPay), result.Id, null, null, cancellationToken);

        return CreatedAtAction(nameof(GetPromise), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PromiseToPayDto>> UpdatePromise(Guid id, UpdatePromiseToPayCommand command, CancellationToken cancellationToken)
    {
        GetTenantId();

        command.Id = id;
        var result = await _sender.Send(command, cancellationToken);
        await _auditService.LogAsync("Update", nameof(Domain.Entities.PromiseToPay), id, null, null, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePromise(Guid id, CancellationToken cancellationToken)
    {
        GetTenantId();

        await _sender.Send(new DeletePromiseToPayCommand { Id = id }, cancellationToken);
        await _auditService.LogAsync("Delete", nameof(Domain.Entities.PromiseToPay), id, null, null, cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/fulfill")]
    public async Task<ActionResult<PromiseToPayDto>> FulfillPromise(Guid id, [FromBody] FulfillPromiseToPayCommand command, CancellationToken cancellationToken)
    {
        GetTenantId();

        command.Id = id;
        var result = await _sender.Send(command, cancellationToken);
        await _auditService.LogAsync("Fulfill", nameof(Domain.Entities.PromiseToPay), id, null, null, cancellationToken);

        return Ok(result);
    }
}
