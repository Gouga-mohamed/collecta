using CollectA.Domain.Entities;
using CollectA.Domain.Enums;
using FluentAssertions;

namespace CollectA.Domain.Tests;

public class PaymentChequeTests
{
    [Fact]
    public void Payment_Method_ShouldDefaultToBankTransfer()
    {
        var payment = new Payment();

        payment.Method.Should().Be(PaymentMethod.BankTransfer);
        payment.Currency.Should().Be("DZD");
    }

    [Fact]
    public void Cheque_Status_ShouldDefaultToReceived()
    {
        var cheque = new Cheque();

        cheque.Status.Should().Be(ChequeStatus.Received);
        cheque.Currency.Should().Be("DZD");
    }

    [Fact]
    public void Invoice_RemainingAmount_ShouldDecrease_WhenPaymentIsApplied()
    {
        var invoice = new Invoice
        {
            Amount = 5_000_000m,
            PaidAmount = 0m,
            DueDate = DateTime.UtcNow.AddDays(30)
        };

        invoice.PaidAmount += 2_000_000m;

        invoice.RemainingAmount.Should().Be(3_000_000m);
    }

    [Fact]
    public void Invoice_RemainingAmount_ShouldBeZero_WhenFullyPaid()
    {
        var invoice = new Invoice
        {
            Amount = 5_000_000m,
            PaidAmount = 5_000_000m,
            DueDate = DateTime.UtcNow.AddDays(30)
        };

        invoice.RemainingAmount.Should().Be(0m);
        invoice.IsOverdue.Should().BeFalse();
    }
}
