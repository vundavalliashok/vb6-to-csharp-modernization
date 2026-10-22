namespace LegacyMigration.Application;

public sealed record CreateCustomerRequest(string CustomerCode, string Name, string? Email);
public sealed record CustomerResponse(int Id, string CustomerCode, string Name, string? Email, DateTime CreatedUtc);
public sealed record CreateOrderLineRequest(string ProductCode, string Description, int Quantity, decimal UnitPrice);
public sealed record CreateOrderRequest(int CustomerId, List<CreateOrderLineRequest> Items);
public sealed record OrderLineResponse(string ProductCode, string Description, int Quantity, decimal UnitPrice, decimal LineTotal);
public sealed record OrderResponse(int Id, int CustomerId, DateTime CreatedUtc, string Status, decimal Total, IReadOnlyList<OrderLineResponse> Items);
