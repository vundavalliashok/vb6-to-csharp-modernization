namespace LegacyMigration.Domain;

public sealed class Customer
{
    public int Id { get; set; }
    public required string CustomerCode { get; set; }
    public required string Name { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
