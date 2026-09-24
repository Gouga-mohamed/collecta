using CollectA.Application.Common.Dtos;
using CollectA.Application.Features.Analytics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollectA.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly ISender _sender;

    public AnalyticsController(ISender sender)
    {
        _sender = sender;
    }

    private Guid GetTenantId()
    {
        var claim = User.FindFirstValue("tenant_id");
        return Guid.Parse(claim!);
    }

    [HttpGet("dso")]
    public async Task<ActionResult<DsoResultDto>> GetDso(
        [FromQuery] int periodDays = 90,
        CancellationToken cancellationToken = default)
    {
        GetTenantId();

        var result = await _sender.Send(new GetDsoQuery { PeriodDays = periodDays <= 0 ? 90 : periodDays }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("cash-forecast")]
    public async Task<ActionResult<CashForecastDto>> GetCashForecast(
        [FromQuery] int periods = 6,
        CancellationToken cancellationToken = default)
    {
        GetTenantId();

        var result = await _sender.Send(new GetCashForecastQuery { Periods = periods <= 0 ? 6 : periods }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("collection-rate")]
    public async Task<ActionResult<CollectionRateDto>> GetCollectionRate(
        [FromQuery] int periodDays = 30,
        CancellationToken cancellationToken = default)
    {
        GetTenantId();

        var result = await _sender.Send(new GetCollectionRateQuery { PeriodDays = periodDays <= 0 ? 30 : periodDays }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("collections")]
    public async Task<ActionResult<CollectionsAnalyticsDto>> GetCollectionsAnalytics(
        [FromQuery] int trendMonths = 12,
        CancellationToken cancellationToken = default)
    {
        GetTenantId();

        var result = await _sender.Send(new GetCollectionsAnalyticsQuery { TrendMonths = trendMonths }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("receivables")]
    public async Task<ActionResult<ReceivablesAnalyticsDto>> GetReceivablesAnalytics(
        CancellationToken cancellationToken = default)
    {
        GetTenantId();

        var result = await _sender.Send(new GetReceivablesAnalyticsQuery(), cancellationToken);
        return Ok(result);
    }
}
