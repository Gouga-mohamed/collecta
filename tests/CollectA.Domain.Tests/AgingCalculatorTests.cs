using CollectA.Domain.Common;
using FluentAssertions;

namespace CollectA.Domain.Tests;

public class AgingCalculatorTests
{
    [Theory]
    [InlineData(0, AgingCalculator.Current)]
    [InlineData(1, AgingCalculator.Days1To30)]
    [InlineData(30, AgingCalculator.Days1To30)]
    [InlineData(31, AgingCalculator.Days31To60)]
    [InlineData(60, AgingCalculator.Days31To60)]
    [InlineData(61, AgingCalculator.Days61To90)]
    [InlineData(90, AgingCalculator.Days61To90)]
    [InlineData(91, AgingCalculator.Days91To120)]
    [InlineData(120, AgingCalculator.Days91To120)]
    [InlineData(121, AgingCalculator.Days121To180)]
    [InlineData(180, AgingCalculator.Days121To180)]
    [InlineData(181, AgingCalculator.Over180)]
    public void GetBucket_ShouldReturnExpectedBucket_WhenDaysOverdueIsAtBoundary(int daysOverdue, string expected)
    {
        AgingCalculator.GetBucket(daysOverdue).Should().Be(expected);
    }

    [Fact]
    public void GetBucket_ShouldReturnCurrent_WhenDaysOverdueIsNegative()
    {
        AgingCalculator.GetBucket(-5).Should().Be(AgingCalculator.Current);
    }

    [Fact]
    public void GetBucket_ShouldReturnOver180_WhenDaysOverdueIsVeryLarge()
    {
        AgingCalculator.GetBucket(500).Should().Be(AgingCalculator.Over180);
    }
}
