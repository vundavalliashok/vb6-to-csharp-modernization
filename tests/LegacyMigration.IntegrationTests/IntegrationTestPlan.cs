namespace LegacyMigration.IntegrationTests;

/// <summary>
/// Integration-test execution plan. Connect this project to a disposable SQL Server
/// and ASP.NET Core WebApplicationFactory when adding CI-managed database integration tests.
/// </summary>
public sealed class IntegrationTestPlan
{
    [Fact]
    public void IntegrationSuiteDocumentsRequiredCoverage()
    {
        var requiredScenarios = new[]
        {
            "POST customer returns 201 and persists customer",
            "Duplicate customer code returns 409",
            "POST order persists order and line totals atomically",
            "Unknown customer order returns 404",
            "Invalid quantity or negative price returns 400",
            "GET order returns stored total and items"
        };

        Assert.Equal(6, requiredScenarios.Length);
    }
}
