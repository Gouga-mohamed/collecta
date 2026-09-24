using CollectA.Application.Common.Dtos;
using CollectA.Application.Features.Dashboard;
using CollectA.Application.Features.Receivables;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollectA.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender)
    {
        _sender = sender;
    }

    private Guid GetTenantId()
    {
        var claim = User.FindFirstValue("tenant_id");
        return Guid.Parse(claim!);
    }

    [HttpGet]
    public async Task<ActionResult<CfoDashboardDto>> GetCfoDashboard(CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(new GetCfoDashboardQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("aging")]
    public async Task<ActionResult<List<AgingBucketDto>>> GetAging(CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(new GetAgingQuery(), cancellationToken);
        return Ok(result);
    }
}
