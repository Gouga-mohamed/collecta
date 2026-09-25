using CollectA.Application.Common.Dtos;
using CollectA.Application.Features.Collections;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollectA.Api.Controllers;

[ApiController]
[Route("api/agent-dashboard")]
[Authorize]
public class AgentDashboardController : ControllerBase
{
    private readonly ISender _sender;

    public AgentDashboardController(ISender sender)
    {
        _sender = sender;
    }

    private Guid GetTenantId()
    {
        var claim = User.FindFirstValue("tenant_id");
        return Guid.Parse(claim!);
    }

    [HttpGet]
    public async Task<ActionResult<AgentDashboardDto>> GetAgentDashboard(CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(new GetAgentDashboardQuery(), cancellationToken);
        return Ok(result);
    }
}
