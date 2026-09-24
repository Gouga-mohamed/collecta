using CollectA.Domain.Common;
using CollectA.Domain.Common.Interfaces;
using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Imports;

public class ConfirmCsvImportCommand : IRequest<CsvImportResultDto>
{
    public Guid ImportId { get; set; }
    public Dictionary<string, string> ColumnMapping { get; set; } = new();
}

public class ConfirmCsvImportCommandHandler : IRequestHandler<ConfirmCsvImportCommand, CsvImportResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public ConfirmCsvImportCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<CsvImportResultDto> Handle(ConfirmCsvImportCommand request, CancellationToken cancellationToken)
    {
        var session = ImportSessionStore.Get(request.ImportId);
        if (session == null)
        {
            throw new KeyNotFoundException($"Import session '{request.ImportId}' not found.");
        }

        var headerIndex = session.Headers
            .Select((h, i) => new { Header = h, Index = i })
            .ToDictionary(x => x.Header, x => x.Index, StringComparer.OrdinalIgnoreCase);

        var mapping = request.ColumnMapping.Count > 0 ? request.ColumnMapping : session.ColumnMapping;

        var result = new CsvImportResultDto();
        var importedInvoices = 0;
        var failedCount = 0;

        var existingCustomers = await _context.Customers
            .ForTenant(_tenantContext)
            .ToDictionaryAsync(c => c.Code, c => c, cancellationToken);

        var existingInvoices = await _context.Invoices
            .ForTenant(_tenantContext)
            .ToDictionaryAsync(i => i.InvoiceNumber, i => i, cancellationToken);

        var customersToAdd = new Dictionary<string, Customer>(StringComparer.OrdinalIgnoreCase);
        var invoicesToAdd = new List<Invoice>();

        for (int i = 0; i < session.Rows.Count; i++)
        {
            var rowNumber = i + 2;
            var rowValues = session.Rows[i];
            var values = GetMappedValues(rowValues, headerIndex, mapping);

            if (!TryGetRequired(values, "CustomerCode", out var customerCode) ||
                !TryGetRequired(values, "CustomerName", out var customerName))
            {
                failedCount++;
                result.Errors.Add($"Row {rowNumber}: Customer code and name are required.");
                continue;
            }

            Customer customer;
            if (existingCustomers.TryGetValue(customerCode, out var existingCustomer))
            {
                customer = existingCustomer;
            }
            else if (customersToAdd.TryGetValue(customerCode, out var newCustomer))
            {
                customer = newCustomer;
            }
            else
            {
                customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    Code = customerCode,
                    Name = customerName,
                    Email = GetOptional(values, "CustomerEmail"),
                    PhoneNumber = GetOptional(values, "CustomerPhone"),
                    Address = GetOptional(values, "CustomerAddress"),
                    City = GetOptional(values, "CustomerCity"),
                    Country = GetOptional(values, "CustomerCountry") ?? "Algeria",
                    PaymentTermsDays = ParseInt(values, "PaymentTermsDays", 30),
                    IsActive = true
                };
                customersToAdd[customerCode] = customer;
            }

            var invoiceNumber = GetOptional(values, "InvoiceNumber");
            if (!string.IsNullOrWhiteSpace(invoiceNumber))
            {
                if (existingInvoices.ContainsKey(invoiceNumber) || invoicesToAdd.Any(inv => inv.InvoiceNumber == invoiceNumber))
                {
                    failedCount++;
                    result.Errors.Add($"Row {rowNumber}: Invoice number '{invoiceNumber}' already exists.");
                    continue;
                }

                if (!TryParseDate(values, "InvoiceDate", out var invoiceDate) ||
                    !TryParseDate(values, "DueDate", out var dueDate) ||
                    !TryParseDecimal(values, "Amount", out var amount))
                {
                    failedCount++;
                    result.Errors.Add($"Row {rowNumber}: Invalid invoice data.");
                    continue;
                }

                var invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    InvoiceNumber = invoiceNumber,
                    CustomerId = customer.Id,
                    Customer = customer,
                    InvoiceDate = invoiceDate,
                    DueDate = dueDate,
                    Amount = amount,
                    PaidAmount = 0,
                    Currency = GetOptional(values, "Currency") ?? "DZD",
                    Description = GetOptional(values, "Description"),
                    PaymentTermsDays = ParseInt(values, "PaymentTermsDays", 30)
                };

                InvoiceStatusCalculator.Recalculate(invoice);
                invoicesToAdd.Add(invoice);
                importedInvoices++;
            }
        }

        _context.Customers.AddRange(customersToAdd.Values);
        _context.Invoices.AddRange(invoicesToAdd);

        await _context.SaveChangesAsync(cancellationToken);
        ImportSessionStore.Remove(request.ImportId);

        result.ImportedCount = customersToAdd.Count + importedInvoices;
        result.FailedCount = failedCount;
        return result;
    }

    private static Dictionary<string, string> GetMappedValues(string[] rowValues, Dictionary<string, int> headerIndex, Dictionary<string, string> mapping)
    {
        var values = new Dictionary<string, string>();
        foreach (var mapped in mapping)
        {
            if (headerIndex.TryGetValue(mapped.Value, out var index) && index < rowValues.Length)
            {
                values[mapped.Key] = rowValues[index];
            }
        }
        return values;
    }

    private static bool TryGetRequired(Dictionary<string, string> values, string key, out string value)
    {
        if (values.TryGetValue(key, out var found) && !string.IsNullOrWhiteSpace(found))
        {
            value = found;
            return true;
        }
        value = string.Empty;
        return false;
    }

    private static string? GetOptional(Dictionary<string, string> values, string key)
    {
        if (values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }
        return null;
    }

    private static int ParseInt(Dictionary<string, string> values, string key, int defaultValue)
    {
        if (values.TryGetValue(key, out var value) && int.TryParse(value, out var result))
        {
            return result;
        }
        return defaultValue;
    }

    private static bool TryParseDate(Dictionary<string, string> values, string key, out DateTime date)
    {
        date = DateTime.MinValue;
        if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return false;
        }
        if (DateTime.TryParse(value, out date))
        {
            date = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            return true;
        }
        return false;
    }

    private static bool TryParseDecimal(Dictionary<string, string> values, string key, out decimal amount)
    {
        amount = 0;
        if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return false;
        }
        return decimal.TryParse(value, out amount) && amount > 0;
    }
}
