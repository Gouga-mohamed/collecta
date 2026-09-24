using CollectA.Domain.Common;
using CollectA.Domain.Entities;
using CollectA.Domain.Enums;
using FluentAssertions;

namespace CollectA.Domain.Tests;

public class InvoiceStatusCalculatorTests
{
    [Fact]
    public void Recalculate_ShouldSetOpen_WhenNothingPaidAndNotDue()
    {
        var invoice = new Invoice
        {
            Amount = 1_000_000m,
            PaidAmount = 0m,
            DueDate = DateTime.UtcNow.AddDays(15)
        };

        InvoiceStatusCalculator.Recalculate(invoice);

        invoice.Status.Should().Be(InvoiceStatus.Open);
    }

    [Fact]
    public void Recalculate_ShouldSetOverdue_WhenNothingPaidAndDueDatePassed()
    {
        var invoice = new Invoice
        {
            Amount = 1_000_000m,
            PaidAmount = 0m,
            DueDate = DateTime.UtcNow.AddDays(-10)
        };

        InvoiceStatusCalculator.Recalculate(invoice);

        invoice.Status.Should().Be(InvoiceStatus.Overdue);
    }

    [Fact]
    public void Recalculate_ShouldSetPartiallyPaid_WhenPaidAmountIsBetweenZeroAndAmount()
    {
        var invoice = new Invoice
        {
            Amount = 2_000_000m,
            PaidAmount = 500_000m,
            DueDate = DateTime.UtcNow.AddDays(20)
        };

        InvoiceStatusCalculator.Recalculate(invoice);

        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
    }

    [Fact]
    public void Recalculate_ShouldSetPartiallyPaid_WhenPaidAmountIsBetweenZeroAndAmountAndInvoiceIsOverdue()
    {
        var invoice = new Invoice
        {
            Amount = 2_000_000m,
            PaidAmount = 500_000m,
            DueDate = DateTime.UtcNow.AddDays(-30)
        };

        InvoiceStatusCalculator.Recalculate(invoice);

        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
    }

    [Fact]
    public void Recalculate_ShouldSetPaid_WhenPaidAmountEqualsAmount()
    {
        var invoice = new Invoice
        {
            Amount = 3_500_000m,
            PaidAmount = 3_500_000m,
            DueDate = DateTime.UtcNow.AddDays(-5)
        };

        InvoiceStatusCalculator.Recalculate(invoice);

        invoice.Status.Should().Be(InvoiceStatus.Paid);
    }

    [Fact]
    public void Recalculate_ShouldSetPaid_WhenPaidAmountExceedsAmount()
    {
        var invoice = new Invoice
        {
            Amount = 3_500_000m,
            PaidAmount = 3_600_000m,
            DueDate = DateTime.UtcNow.AddDays(10)
        };

        InvoiceStatusCalculator.Recalculate(invoice);

        invoice.Status.Should().Be(InvoiceStatus.Paid);
    }

    [Fact]
    public void Recalculate_ShouldSetDisputed_WhenUnpaidNotOverdueAndIsDisputed()
    {
        var invoice = new Invoice
        {
            Amount = 4_200_000m,
            PaidAmount = 0m,
            DueDate = DateTime.UtcNow.AddDays(35),
            IsDisputed = true,
            DisputedAt = DateTime.UtcNow.AddDays(-5)
        };

        InvoiceStatusCalculator.Recalculate(invoice);

        invoice.Status.Should().Be(InvoiceStatus.Disputed);
    }

    [Fact]
    public void Recalculate_ShouldKeepCancelled_WhenInvoiceIsCancelled()
    {
        var invoice = new Invoice
        {
            Amount = 1_000_000m,
            PaidAmount = 0m,
            DueDate = DateTime.UtcNow.AddDays(-10),
            Status = InvoiceStatus.Cancelled
        };

        InvoiceStatusCalculator.Recalculate(invoice);

        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    [Fact]
    public void Recalculate_ShouldKeepWrittenOff_WhenInvoiceIsWrittenOff()
    {
        var invoice = new Invoice
        {
            Amount = 1_000_000m,
            PaidAmount = 0m,
            DueDate = DateTime.UtcNow.AddDays(-200),
            Status = InvoiceStatus.WrittenOff
        };

        InvoiceStatusCalculator.Recalculate(invoice);

        invoice.Status.Should().Be(InvoiceStatus.WrittenOff);
    }
}
