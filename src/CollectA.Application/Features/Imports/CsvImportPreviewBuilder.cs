using CollectA.Application.Common.Dtos;

namespace CollectA.Application.Features.Imports;

public static class CsvImportPreviewBuilder
{
    public static CsvImportPreviewDto Build(Guid importId, string fileName, List<string> headers, List<string[]> rows, Dictionary<string, string> mapping)
    {
        var headerIndex = headers
            .Select((h, i) => new { Header = h, Index = i })
            .ToDictionary(x => x.Header, x => x.Index, StringComparer.OrdinalIgnoreCase);

        var previewRows = new List<CsvImportRowDto>();
        var errors = new List<CsvImportErrorDto>();
        var validRows = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            var rowNumber = i + 2; // 1-based with header at row 1
            var rowValues = rows[i];
            var values = new Dictionary<string, string>();
            var rowErrors = new List<string>();

            foreach (var mapped in mapping)
            {
                if (headerIndex.TryGetValue(mapped.Value, out var index) && index < rowValues.Length)
                {
                    values[mapped.Key] = rowValues[index];
                }
            }

            ValidateRow(values, rowNumber, rowErrors, errors);

            var isValid = !rowErrors.Any();
            if (isValid) validRows++;

            previewRows.Add(new CsvImportRowDto
            {
                RowNumber = rowNumber,
                Values = values,
                IsValid = isValid,
                Errors = rowErrors
            });
        }

        return new CsvImportPreviewDto
        {
            ImportId = importId,
            FileName = fileName,
            TotalRows = rows.Count,
            ValidRows = validRows,
            ErrorRows = rows.Count - validRows,
            Headers = headers,
            Rows = previewRows,
            Errors = errors,
            ColumnMapping = mapping
        };
    }

    private static void ValidateRow(Dictionary<string, string> values, int rowNumber, List<string> rowErrors, List<CsvImportErrorDto> errors)
    {
        void AddError(string column, string message)
        {
            rowErrors.Add($"{column}: {message}");
            errors.Add(new CsvImportErrorDto { RowNumber = rowNumber, Column = column, Message = message });
        }

        if (!values.TryGetValue("CustomerCode", out var customerCode) || string.IsNullOrWhiteSpace(customerCode))
        {
            AddError("CustomerCode", "Customer code is required.");
        }

        if (!values.TryGetValue("CustomerName", out var customerName) || string.IsNullOrWhiteSpace(customerName))
        {
            AddError("CustomerName", "Customer name is required.");
        }

        if (values.TryGetValue("InvoiceNumber", out var invoiceNumber) && !string.IsNullOrWhiteSpace(invoiceNumber))
        {
            if (values.TryGetValue("InvoiceDate", out var invoiceDateStr) && !string.IsNullOrWhiteSpace(invoiceDateStr))
            {
                if (!DateTime.TryParse(invoiceDateStr, out _))
                {
                    AddError("InvoiceDate", "Invalid date format.");
                }
            }

            if (values.TryGetValue("DueDate", out var dueDateStr) && !string.IsNullOrWhiteSpace(dueDateStr))
            {
                if (!DateTime.TryParse(dueDateStr, out _))
                {
                    AddError("DueDate", "Invalid date format.");
                }
            }

            if (values.TryGetValue("Amount", out var amountStr) && !string.IsNullOrWhiteSpace(amountStr))
            {
                if (!decimal.TryParse(amountStr, out var amount) || amount <= 0)
                {
                    AddError("Amount", "Amount must be a positive number.");
                }
            }
        }
    }
}
