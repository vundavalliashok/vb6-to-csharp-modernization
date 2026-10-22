using LegacyMigration.Domain;

namespace LegacyMigration.UnitTests;

public sealed class LegacyOrderCalculatorTests
{
    // Golden-master examples for the representative legacy behavior.
    [Theory]
    [InlineData(1, 10.00, 10.0000)]
    [InlineData(3, 19.95, 59.8500)]
    [InlineData(2, 0.125, 0.2500)]
    [InlineData(4, 1.2345, 4.9380)]
    public void CalculateLineTotal_MatchesLegacyRegressionCases(int quantity, decimal unitPrice, decimal expected)
    {
        Assert.Equal(expected, LegacyOrderCalculator.CalculateLineTotal(quantity, unitPrice));
    }

    [Fact]
    public void CalculateOrderTotal_SumsLineTotals()
    {
        var total = LegacyOrderCalculator.CalculateOrderTotal(new[] { (2, 19.95m), (1, 10m) });
        Assert.Equal(49.9000m, total);
    }

    [Fact]
    public void CalculateLineTotal_RejectsZeroQuantity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LegacyOrderCalculator.CalculateLineTotal(0, 10m));
    }

    [Fact]
    public void CalculateLineTotal_RejectsNegativeUnitPrice()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LegacyOrderCalculator.CalculateLineTotal(1, -0.01m));
    }
}
