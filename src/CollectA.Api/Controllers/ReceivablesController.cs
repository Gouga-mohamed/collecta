using CollectA.Application.Common.Dtos;
using CollectA.Application.Features.Receivables;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollectA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceivablesController : ControllerBase
{
    private readonly ISender _sender;

    public ReceivablesController(ISender sender)
    {
        _sender = sender;
    }

    private Guid GetTenantId()
    {
        var claim = User.FindFirstValue("tenant_id");
        return Guid.Parse(claim!);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ReceivablesSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(new GetReceivablesSummaryQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("aging")]
    public async Task<ActionResult<List<AgingBucketDto>>> GetAging(CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(new GetAgingQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<List<OverdueInvoiceDto>>> GetOverdue(
        [FromQuery] int? minDays = null,
        [FromQuery] int? maxDays = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        GetTenantId();

        var query = new GetOverdueInvoicesQuery
        {
            MinDays = minDays,
            MaxDays = maxDays,
            CustomerId = customerId,
            Take = take
        };

        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }
}
