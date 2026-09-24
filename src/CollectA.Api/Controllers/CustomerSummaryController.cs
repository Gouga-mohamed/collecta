using CollectA.Application.Common.Dtos;
using CollectA.Application.Features.Customers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollectA.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomerSummaryController : ControllerBase
{
    private readonly ISender _sender;

    public CustomerSummaryController(ISender sender)
    {
        _sender = sender;
    }

    private Guid GetTenantId()
    {
        var claim = User.FindFirstValue("tenant_id");
        return Guid.Parse(claim!);
    }

    [HttpGet("{id:guid}/summary")]
    public async Task<ActionResult<Customer360Dto>> GetSummary(Guid id, CancellationToken cancellationToken)
    {
        GetTenantId();

        var result = await _sender.Send(new GetCustomer360Query { Id = id }, cancellationToken);
        if (result == null) return NotFound();

        return Ok(result);
    }
}
