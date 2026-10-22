namespace LegacyMigration.Domain;

/// <summary>
/// Reimplementation of the representative VB6 Currency-based order calculation.
/// Confirm rounding/overflow behavior against the actual legacy executable before migration cutover.
/// </summary>
public static class LegacyOrderCalculator
{
    public static decimal CalculateLineTotal(int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        if (unitPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");

        return decimal.Round(quantity * unitPrice, 4, MidpointRounding.ToEven);
    }

    public static decimal CalculateOrderTotal(IEnumerable<(int Quantity, decimal UnitPrice)> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        return lines.Sum(line => CalculateLineTotal(line.Quantity, line.UnitPrice));
    }
}
