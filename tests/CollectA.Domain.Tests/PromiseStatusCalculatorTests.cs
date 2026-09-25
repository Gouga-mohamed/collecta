using CollectA.Domain.Common;
using CollectA.Domain.Entities;
using CollectA.Domain.Enums;
using FluentAssertions;

namespace CollectA.Domain.Tests;

public class PromiseStatusCalculatorTests
{
    [Fact]
    public void Calculate_ShouldReturnFulfilled_WhenFulfilledAmountEqualsPromisedAmount()
    {
        var promise = new PromiseToPay { PromisedAmount = 1_000_000m };

        var status = PromiseStatusCalculator.Calculate(promise, 1_000_000m);

        status.Should().Be(PromiseStatus.Fulfilled);
    }

    [Fact]
    public void Calculate_ShouldReturnFulfilled_WhenFulfilledAmountExceedsPromisedAmount()
    {
        var promise = new PromiseToPay { PromisedAmount = 1_000_000m };

        var status = PromiseStatusCalculator.Calculate(promise, 1_200_000m);

        status.Should().Be(PromiseStatus.Fulfilled);
    }

    [Fact]
    public void Calculate_ShouldReturnPartiallyFulfilled_WhenFulfilledAmountIsBetweenZeroAndPromisedAmount()
    {
        var promise = new PromiseToPay { PromisedAmount = 1_000_000m };

        var status = PromiseStatusCalculator.Calculate(promise, 500_000m);

        status.Should().Be(PromiseStatus.PartiallyFulfilled);
    }

    [Fact]
    public void Calculate_ShouldReturnBroken_WhenFulfilledAmountIsZero()
    {
        var promise = new PromiseToPay { PromisedAmount = 1_000_000m };

        var status = PromiseStatusCalculator.Calculate(promise, 0m);

        status.Should().Be(PromiseStatus.Broken);
    }

    [Fact]
    public void Calculate_ShouldKeepCancelled_WhenPromiseIsCancelled()
    {
        var promise = new PromiseToPay { PromisedAmount = 1_000_000m, Status = PromiseStatus.Cancelled };

        var status = PromiseStatusCalculator.Calculate(promise, 1_000_000m);

        status.Should().Be(PromiseStatus.Cancelled);
    }
}
