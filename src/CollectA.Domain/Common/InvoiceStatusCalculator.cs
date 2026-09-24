using CollectA.Domain.Entities;
using CollectA.Domain.Enums;

namespace CollectA.Domain.Common;

public static class InvoiceStatusCalculator
{
    public static void Recalculate(Invoice invoice)
    {
        if (invoice.Status == InvoiceStatus.Cancelled || invoice.Status == InvoiceStatus.WrittenOff) return;

        if (invoice.PaidAmount >= invoice.Amount)
        {
            invoice.Status = InvoiceStatus.Paid;
        }
        else if (invoice.PaidAmount > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }
        else if (invoice.DueDate < DateTime.UtcNow.Date)
        {
            invoice.Status = InvoiceStatus.Overdue;
        }
        else if (invoice.IsDisputed)
        {
            invoice.Status = InvoiceStatus.Disputed;
        }
        else
        {
            invoice.Status = InvoiceStatus.Open;
        }
    }
}
