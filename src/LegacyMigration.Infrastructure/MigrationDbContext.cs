using LegacyMigration.Domain;
using Microsoft.EntityFrameworkCore;

namespace LegacyMigration.Infrastructure;

public sealed class MigrationDbContext(DbContextOptions<MigrationDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesOrder> Orders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> OrderLines => Set<SalesOrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CustomerCode).IsUnique();
            entity.Property(x => x.CustomerCode).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(320);
        });

        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Total).HasPrecision(19, 4);
            entity.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.SalesOrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SalesOrderLine>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(250).IsRequired();
            entity.Property(x => x.UnitPrice).HasPrecision(19, 4);
            entity.Property(x => x.LineTotal).HasPrecision(19, 4);
        });
    }
}
