using CollectA.Domain.Common;
using CollectA.Domain.Entities;
using CollectA.Domain.Enums;
using FluentAssertions;

namespace CollectA.Domain.Tests;

public class DisputeWorkflowTests
{
    [Theory]
    [InlineData(DisputeStatus.Open, DisputeStatus.Investigating, true)]
    [InlineData(DisputeStatus.Investigating, DisputeStatus.WaitingCustomer, true)]
    [InlineData(DisputeStatus.WaitingCustomer, DisputeStatus.Open, true)]
    [InlineData(DisputeStatus.Open, DisputeStatus.Resolved, true)]
    [InlineData(DisputeStatus.Investigating, DisputeStatus.Closed, true)]
    [InlineData(DisputeStatus.Resolved, DisputeStatus.Open, false)]
    [InlineData(DisputeStatus.Closed, DisputeStatus.Investigating, false)]
    public void CanTransitionTo_ShouldReturnExpectedResult_WhenTransitioning(DisputeStatus from, DisputeStatus to, bool expected)
    {
        var result = DisputeWorkflow.CanTransitionTo(from, to);

        result.Should().Be(expected);
    }

    [Fact]
    public void IsActive_ShouldBeTrue_WhenStatusIsOpenOrInvestigatingOrWaiting()
    {
        DisputeWorkflow.IsActive(DisputeStatus.Open).Should().BeTrue();
        DisputeWorkflow.IsActive(DisputeStatus.Investigating).Should().BeTrue();
        DisputeWorkflow.IsActive(DisputeStatus.WaitingCustomer).Should().BeTrue();
        DisputeWorkflow.IsActive(DisputeStatus.WaitingInternal).Should().BeTrue();
    }

    [Fact]
    public void IsActive_ShouldBeFalse_WhenStatusIsResolvedOrClosed()
    {
        DisputeWorkflow.IsActive(DisputeStatus.Resolved).Should().BeFalse();
        DisputeWorkflow.IsActive(DisputeStatus.Closed).Should().BeFalse();
    }

    [Fact]
    public void InvoiceStatusCalculator_ShouldSetDisputed_WhenInvoiceIsDisputedAndUnpaid()
    {
        var invoice = new Invoice
        {
            Amount = 1_000_000m,
            PaidAmount = 0m,
            DueDate = DateTime.UtcNow.AddDays(15),
            IsDisputed = true,
            DisputedAt = DateTime.UtcNow.AddDays(-5)
        };

        InvoiceStatusCalculator.Recalculate(invoice);

        invoice.Status.Should().Be(InvoiceStatus.Disputed);
    }

    [Fact]
    public void InvoiceStatusCalculator_ShouldSetOpen_WhenDisputeIsResolved()
    {
        var invoice = new Invoice
        {
            Amount = 1_000_000m,
            PaidAmount = 0m,
            DueDate = DateTime.UtcNow.AddDays(15),
            IsDisputed = false
        };

        InvoiceStatusCalculator.Recalculate(invoice);

        invoice.Status.Should().Be(InvoiceStatus.Open);
    }

    [Fact]
    public void InvoiceStatusCalculator_ShouldSetOverdue_WhenDisputeIsResolvedAndDueDatePassed()
    {
        var invoice = new Invoice
        {
            Amount = 1_000_000m,
            PaidAmount = 0m,
            DueDate = DateTime.UtcNow.AddDays(-10),
            IsDisputed = false
        };

        InvoiceStatusCalculator.Recalculate(invoice);

        invoice.Status.Should().Be(InvoiceStatus.Overdue);
    }
}
