namespace CollectA.Application.Common.Dtos;

public class CsvImportRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentBase64 { get; set; } = string.Empty;
}

public class CsvImportPreviewDto
{
    public Guid ImportId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int ErrorRows { get; set; }
    public List<string> Headers { get; set; } = new();
    public List<CsvImportRowDto> Rows { get; set; } = new();
    public List<CsvImportErrorDto> Errors { get; set; } = new();
    public Dictionary<string, string> ColumnMapping { get; set; } = new();
}

public class CsvImportRowDto
{
    public int RowNumber { get; set; }
    public Dictionary<string, string> Values { get; set; } = new();
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class CsvImportErrorDto
{
    public int RowNumber { get; set; }
    public string Column { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class CsvImportMappingRequest
{
    public Guid ImportId { get; set; }
    public Dictionary<string, string> ColumnMapping { get; set; } = new();
}

public class CsvImportResultDto
{
    public int ImportedCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
