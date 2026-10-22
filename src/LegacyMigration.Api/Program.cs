using LegacyMigration.Application;
using LegacyMigration.Domain;
using LegacyMigration.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

builder.Services.AddDbContext<MigrationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Convenience for local demo only. Use reviewed EF migrations/SQL scripts outside Development.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MigrationDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy", utc = DateTime.UtcNow }))
   .WithName("Health");

app.MapGet("/api/customers", async (MigrationDbContext db, CancellationToken ct) =>
    Results.Ok(await db.Customers.AsNoTracking().OrderBy(c => c.Id)
        .Select(c => new CustomerResponse(c.Id, c.CustomerCode, c.Name, c.Email, c.CreatedUtc))
        .ToListAsync(ct)));

app.MapGet("/api/customers/{id:int}", async (int id, MigrationDbContext db, CancellationToken ct) =>
{
    var customer = await db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
    return customer is null
        ? Results.NotFound()
        : Results.Ok(new CustomerResponse(customer.Id, customer.CustomerCode, customer.Name, customer.Email, customer.CreatedUtc));
});

app.MapPost("/api/customers", async (CreateCustomerRequest request, MigrationDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.CustomerCode) || string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new { error = "CustomerCode and Name are required." });
    if (request.CustomerCode.Length > 30 || request.Name.Length > 200 || (request.Email?.Length ?? 0) > 320)
        return Results.BadRequest(new { error = "One or more fields exceed the supported length." });
    var code = request.CustomerCode.Trim();
    if (await db.Customers.AnyAsync(c => c.CustomerCode == code, ct))
        return Results.Conflict(new { error = "CustomerCode already exists." });

    var customer = new Customer { CustomerCode = code, Name = request.Name.Trim(), Email = request.Email?.Trim() };
    db.Customers.Add(customer);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/customers/{customer.Id}",
        new CustomerResponse(customer.Id, customer.CustomerCode, customer.Name, customer.Email, customer.CreatedUtc));
});

app.MapPost("/api/orders", async (CreateOrderRequest request, MigrationDbContext db, CancellationToken ct) =>
{
    if (request.CustomerId <= 0 || request.Items is null || request.Items.Count == 0)
        return Results.BadRequest(new { error = "A valid CustomerId and at least one item are required." });
    if (!await db.Customers.AnyAsync(c => c.Id == request.CustomerId, ct))
        return Results.NotFound(new { error = "Customer not found." });
    if (request.Items.Any(i => string.IsNullOrWhiteSpace(i.ProductCode) ||
                               string.IsNullOrWhiteSpace(i.Description) ||
                               i.ProductCode.Length > 50 || i.Description.Length > 250 ||
                               i.Quantity <= 0 || i.UnitPrice < 0))
        return Results.BadRequest(new { error = "Each item requires product details, positive quantity, and non-negative unit price." });

    var order = new SalesOrder { CustomerId = request.CustomerId };
    foreach (var item in request.Items)
    {
        var lineTotal = LegacyOrderCalculator.CalculateLineTotal(item.Quantity, item.UnitPrice);
        order.Items.Add(new SalesOrderLine {
            ProductCode = item.ProductCode.Trim(),
            Description = item.Description.Trim(),
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = lineTotal
        });
    }
    order.Total = order.Items.Sum(i => i.LineTotal);
    db.Orders.Add(order);
    await db.SaveChangesAsync(ct);

    var response = new OrderResponse(order.Id, order.CustomerId, order.CreatedUtc, order.Status, order.Total,
        order.Items.Select(i => new OrderLineResponse(i.ProductCode, i.Description, i.Quantity, i.UnitPrice, i.LineTotal)).ToList());
    return Results.Created($"/api/orders/{order.Id}", response);
});

app.MapGet("/api/orders/{id:int}", async (int id, MigrationDbContext db, CancellationToken ct) =>
{
    var order = await db.Orders.AsNoTracking().Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);
    return order is null
        ? Results.NotFound()
        : Results.Ok(new OrderResponse(order.Id, order.CustomerId, order.CreatedUtc, order.Status, order.Total,
            order.Items.Select(i => new OrderLineResponse(i.ProductCode, i.Description, i.Quantity, i.UnitPrice, i.LineTotal)).ToList()));
});

app.Run();

public partial class Program { }
