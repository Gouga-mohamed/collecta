using CollectA.Domain.Entities;
using CollectA.Domain.Enums;
using FluentAssertions;

namespace CollectA.Domain.Tests;

public class InvoiceTests
{
    [Fact]
    public void DaysOverdue_ShouldBeZero_WhenInvoiceIsPaid()
    {
        var invoice = new Invoice
        {
            Amount = 1000000,
            PaidAmount = 1000000,
            DueDate = DateTime.UtcNow.AddDays(-10)
        };

        invoice.DaysOverdue.Should().Be(0);
        invoice.IsOverdue.Should().BeFalse();
    }

    [Fact]
    public void DaysOverdue_ShouldBePositive_WhenDueDateIsPassedAndUnpaid()
    {
        var invoice = new Invoice
        {
            Amount = 1000000,
            PaidAmount = 0,
            DueDate = DateTime.UtcNow.AddDays(-15)
        };

        invoice.DaysOverdue.Should().BeGreaterThan(0);
        invoice.IsOverdue.Should().BeTrue();
    }

    [Fact]
    public void RemainingAmount_ShouldBeAmountMinusPaidAmount()
    {
        var invoice = new Invoice
        {
            Amount = 5000000,
            PaidAmount = 1500000
        };

        invoice.RemainingAmount.Should().Be(3500000);
    }

    [Fact]
    public void Status_ShouldDefaultToOpen()
    {
        var invoice = new Invoice();

        invoice.Status.Should().Be(InvoiceStatus.Open);
    }
}
