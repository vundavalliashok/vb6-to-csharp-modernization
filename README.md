# VB6 to C# .NET 10 Modernization

A portfolio-ready sample demonstrating migration of a legacy Visual Basic 6 Customer & Order Management application to an ASP.NET Core REST API backed by SQL Server.

## What is included

- `legacy-vb6/`: representative VB6 modules showing legacy business logic and ADO-style data access.
- `src/LegacyMigration.Domain/`: domain models and legacy-compatible calculation rules.
- `src/LegacyMigration.Application/`: application service contracts and DTOs.
- `src/LegacyMigration.Infrastructure/`: Entity Framework Core SQL Server persistence.
- `src/LegacyMigration.Api/`: ASP.NET Core API, Swagger, validation, health endpoint, and development database initialization.
- `tests/LegacyMigration.UnitTests/`: business-rule and regression tests.
- `tests/LegacyMigration.IntegrationTests/`: API/database integration test scaffold.
- `database/`: SQL Server schema and sample data.
- `.github/workflows/dotnet-ci.yml`: restore, build, test, and publish validation.
- `docs/`: migration mapping, regression strategy, and rollback plan.

## Prerequisites

- .NET 10 SDK
- Docker Desktop (recommended for SQL Server container) or a reachable SQL Server instance
- Git

Check your SDK:

```powershell
dotnet --info
```

## Run locally (PowerShell)

### 1. Start SQL Server in Docker

This is a local-development example. Change the password before using this outside your own machine.

```powershell
docker run --name vb6-migration-sql `
  -e "ACCEPT_EULA=Y" `
  -e "MSSQL_SA_PASSWORD=LocalDev_Only!ChangeMe123" `
  -p 1433:1433 `
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Wait until SQL Server is ready:

```powershell
docker logs -f vb6-migration-sql
```

Press Ctrl+C after the logs indicate SQL Server is ready.

If the container already exists, start it with `docker start vb6-migration-sql`.

### 2. Configure the connection string

The API uses `ConnectionStrings:DefaultConnection`. The repository's development settings use the Docker example above. For a different SQL Server, set an environment variable:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=LegacyMigrationDb;User Id=sa;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=True"
```

Do not commit production passwords or connection strings.

### 3. Restore, build, and test

From the repository root:

```powershell
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

### 4. Run the API

```powershell
dotnet run --project src/LegacyMigration.Api
```

Open the Swagger URL printed in the console (typically `https://localhost:7xxx/swagger` or `http://localhost:5xxx/swagger`).

The API creates the sample database schema using EF Core `EnsureCreated()` only in Development. For production, use reviewed EF Core migrations or controlled SQL deployment scripts instead.

## Example API calls

Create a customer:

```powershell
$customer = @{
  customerCode = "CUST-1001"
  name = "Contoso Manufacturing"
  email = "orders@contoso.example"
} | ConvertTo-Json

Invoke-RestMethod -Method Post `
  -Uri "http://localhost:5000/api/customers" `
  -ContentType "application/json" `
  -Body $customer
```

List customers:

```powershell
Invoke-RestMethod "http://localhost:5000/api/customers"
```

Create an order (replace `customerId` with a real ID returned above):

```powershell
$order = @{
  customerId = 1
  items = @(
    @{ productCode = "PART-01"; description = "Replacement part"; quantity = 2; unitPrice = 19.95 }
  )
} | ConvertTo-Json -Depth 5

Invoke-RestMethod -Method Post `
  -Uri "http://localhost:5000/api/orders" `
  -ContentType "application/json" `
  -Body $order
```

Health endpoint:

```powershell
Invoke-RestMethod "http://localhost:5000/health"
```

## API endpoints

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/health` | Liveness check |
| GET | `/api/customers` | List customers |
| GET | `/api/customers/{id}` | Get a customer |
| POST | `/api/customers` | Create a customer |
| GET | `/api/orders/{id}` | Get an order |
| POST | `/api/orders` | Create an order |

## Migration principles demonstrated

1. Capture current behavior before changing code.
2. Convert VB6 `Currency` calculations carefully; `decimal` is a sensible C# representation, but rounding and overflow behavior must be verified against the actual legacy program.
3. Separate domain rules from UI, persistence, and transport concerns.
4. Replace string-concatenated SQL with parameterized data access.
5. Use regression tests to compare known legacy outputs with the migrated implementation.
6. Deploy with observability, staged rollout, database backup, and a tested rollback plan.

## Production hardening before real use

This is a learning/portfolio sample, not a production-ready business system. Before production use, add authentication/authorization, rate limiting, centralized secret management, structured audit logging, migration scripts, backup/restore verification, CI security scanning, and environment-specific deployment approvals. Review tax, discounts, currency scale, concurrency, and transaction requirements against the real VB6 system.

## Suggested portfolio description

**VB6 to C# .NET Application Modernization** — Designed a sample modernization path from a legacy VB6 customer/order application to a layered ASP.NET Core API using C#, EF Core, and SQL Server. Added business-rule regression tests, API integration test scaffolding, containerized local SQL Server development, and GitHub Actions CI to validate build and test quality gates.
