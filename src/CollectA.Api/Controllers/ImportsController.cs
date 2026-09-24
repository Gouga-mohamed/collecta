using CollectA.Application.Common.Dtos;
using CollectA.Application.Features.Imports;
using CollectA.Domain.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CollectA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImportsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IAuditService _auditService;

    public ImportsController(ISender sender, IAuditService auditService)
    {
        _sender = sender;
        _auditService = auditService;
    }

    private Guid GetTenantId()
    {
        var claim = User.FindFirstValue("tenant_id");
        return Guid.Parse(claim!);
    }

    [HttpPost("csv")]
    public async Task<ActionResult<CsvImportPreviewDto>> UploadCsv(CsvImportRequest request, CancellationToken cancellationToken)
    {
        GetTenantId();

        var command = new UploadCsvCommand
        {
            FileName = request.FileName,
            ContentBase64 = request.ContentBase64
        };

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{sessionId:guid}/preview")]
    public async Task<ActionResult<CsvImportPreviewDto>> Preview(Guid sessionId, [FromQuery] Dictionary<string, string>? mapping = null, CancellationToken cancellationToken = default)
    {
        GetTenantId();

        var query = new PreviewCsvImportQuery
        {
            ImportId = sessionId,
            ColumnMapping = mapping ?? new Dictionary<string, string>()
        };

        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{sessionId:guid}/confirm")]
    public async Task<ActionResult<CsvImportResultDto>> Confirm(Guid sessionId, CsvImportMappingRequest? request, CancellationToken cancellationToken)
    {
        GetTenantId();

        var command = new ConfirmCsvImportCommand
        {
            ImportId = sessionId,
            ColumnMapping = request?.ColumnMapping ?? new Dictionary<string, string>()
        };

        var result = await _sender.Send(command, cancellationToken);
        await _auditService.LogAsync("Import", "CsvImport", sessionId, null, null, cancellationToken);

        return Ok(result);
    }
}
