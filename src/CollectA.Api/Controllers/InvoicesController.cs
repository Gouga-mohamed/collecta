using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Models;
using CollectA.Application.Features.Invoices;
using CollectA.Domain.Common.Interfaces;
using CollectA.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollectA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IAuditService _auditService;

    public InvoicesController(ISender sender, IAuditService auditService)
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
    public async Task<ActionResult<PagedResult<InvoiceDto>>> GetInvoices(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] Guid? customerId = null,
        [FromQuery] InvoiceStatus? status = null,
        [FromQuery] bool? isOverdue = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        GetTenantId();

        var query = new GetInvoicesQuery
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
            IsOverdue = isOverdue,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceDetailDto>> GetInvoice(Guid id, CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(new GetInvoiceByIdQuery { Id = id }, cancellationToken);
        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(command, cancellationToken);
        await _auditService.LogAsync("Create", nameof(Domain.Entities.Invoice), result.Id, null, null, cancellationToken);

        return CreatedAtAction(nameof(GetInvoice), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InvoiceDto>> UpdateInvoice(Guid id, UpdateInvoiceCommand command, CancellationToken cancellationToken)
    {
        GetTenantId();

        command.Id = id;
        var result = await _sender.Send(command, cancellationToken);
        await _auditService.LogAsync("Update", nameof(Domain.Entities.Invoice), id, null, null, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteInvoice(Guid id, CancellationToken cancellationToken)
    {
        GetTenantId();

        await _sender.Send(new DeleteInvoiceCommand { Id = id }, cancellationToken);
        await _auditService.LogAsync("Delete", nameof(Domain.Entities.Invoice), id, null, null, cancellationToken);

        return NoContent();
    }
}
