using CollectA.Application.Common.Dtos;
using MediatR;

namespace CollectA.Application.Features.Imports;

public class PreviewCsvImportQuery : IRequest<CsvImportPreviewDto>
{
    public Guid ImportId { get; set; }
    public Dictionary<string, string> ColumnMapping { get; set; } = new();
}

public class PreviewCsvImportQueryHandler : IRequestHandler<PreviewCsvImportQuery, CsvImportPreviewDto>
{
    public Task<CsvImportPreviewDto> Handle(PreviewCsvImportQuery request, CancellationToken cancellationToken)
    {
        var session = ImportSessionStore.Get(request.ImportId);
        if (session == null)
        {
            throw new KeyNotFoundException($"Import session '{request.ImportId}' not found.");
        }

        var preview = CsvImportPreviewBuilder.Build(session.Id, session.FileName, session.Headers, session.Rows, request.ColumnMapping);
        return Task.FromResult(preview);
    }
}
