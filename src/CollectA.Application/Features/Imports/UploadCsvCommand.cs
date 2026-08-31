using System.Text;
using CollectA.Application.Common.Dtos;
using MediatR;

namespace CollectA.Application.Features.Imports;

public class UploadCsvCommand : IRequest<CsvImportPreviewDto>
{
    public string FileName { get; set; } = string.Empty;
    public string ContentBase64 { get; set; } = string.Empty;
}

public class UploadCsvCommandHandler : IRequestHandler<UploadCsvCommand, CsvImportPreviewDto>
{
    public Task<CsvImportPreviewDto> Handle(UploadCsvCommand request, CancellationToken cancellationToken)
    {
        var csvText = Encoding.UTF8.GetString(Convert.FromBase64String(request.ContentBase64));
        var lines = csvText.Split('\n')
            .Select(l => l.TrimEnd('\r'))
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        if (lines.Count == 0)
        {
            throw new InvalidOperationException("CSV file is empty.");
        }

        var headers = ParseLine(lines[0]);
        var rows = lines.Skip(1)
            .Select(ParseLine)
            .ToList();

        var importId = Guid.NewGuid();
        var session = new ImportSession
        {
            Id = importId,
            FileName = request.FileName,
            Headers = headers,
            Rows = rows
        };
        ImportSessionStore.Set(session);

        var defaultMapping = BuildDefaultMapping(headers);
        var preview = CsvImportPreviewBuilder.Build(importId, request.FileName, headers, rows, defaultMapping);

        return Task.FromResult(preview);
    }

    private static List<string> ParseLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        values.Add(current.ToString().Trim());
        return values;
    }

    private static Dictionary<string, string> BuildDefaultMapping(List<string> headers)
    {
        var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var knownColumns = new[]
        {
            "CustomerCode", "CustomerName", "CustomerEmail", "CustomerPhone",
            "CustomerAddress", "CustomerCity", "CustomerCountry", "PaymentTermsDays",
            "InvoiceNumber", "InvoiceDate", "DueDate", "Amount", "Currency", "Description"
        };

        foreach (var header in headers)
        {
            var normalized = header.Trim().Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
            foreach (var known in knownColumns)
            {
                var knownNormalized = known.ToLowerInvariant();
                if (normalized == knownNormalized ||
                    normalized == knownNormalized.Replace("customer", "").Replace("invoice", ""))
                {
                    mapping[known] = header;
                    break;
                }
            }
        }

        return mapping;
    }
}
