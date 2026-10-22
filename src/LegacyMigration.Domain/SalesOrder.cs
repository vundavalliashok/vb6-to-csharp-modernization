namespace LegacyMigration.Domain;

public sealed class SalesOrder
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "New";
    public decimal Total { get; set; }
    public List<SalesOrderLine> Items { get; set; } = [];
}

public sealed class SalesOrderLine
{
    public int Id { get; set; }
    public int SalesOrderId { get; set; }
    public required string ProductCode { get; set; }
    public required string Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
