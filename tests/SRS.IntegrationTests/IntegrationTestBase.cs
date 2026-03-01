using SRS.Tests.Shared;

namespace SRS.IntegrationTests;

/// <summary>
/// Base for integration tests. Creates TestWebApplicationFactory with shared Postgres and exposes Client (as Admin).
/// Dispose the factory when the test class is disposed.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    protected HttpClient Client { get; }
    protected TestWebApplicationFactory Factory => _factory;

    protected IntegrationTestBase(PostgresFixture postgres)
    {
        _factory = new TestWebApplicationFactory(postgres.ConnectionString);
        Client = _factory.CreateClient();
        Client.AsAdmin();
    }

    public void Dispose() => _factory?.Dispose();
}
