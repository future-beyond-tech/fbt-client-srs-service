using System.Net.Http.Headers;

namespace SRS.Tests.Shared;

/// <summary>
/// HttpClient extensions to simulate authenticated requests in integration tests.
/// Uses X-Test-Role header with Test auth scheme (no secrets, no PII).
/// </summary>
public static class HttpClientAuthExtensions
{
    /// <summary>Add header so request is authenticated as Admin (Role=Admin).</summary>
    public static HttpClient AsAdmin(this HttpClient client)
    {
        client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, TestAuthHandler.AdminRole);
        return client;
    }

    /// <summary>Add header so request is authenticated as non-Admin user (for 403 tests).</summary>
    public static HttpClient AsUser(this HttpClient client)
    {
        client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, TestAuthHandler.UserRole);
        return client;
    }

    /// <summary>Remove auth header so request is anonymous (for 401 tests).</summary>
    public static HttpClient AsAnonymous(this HttpClient client)
    {
        client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);
        client.DefaultRequestHeaders.Remove("Authorization");
        return client;
    }

    /// <summary>Create a new HttpClient with the same base address but no auth (for one-off anonymous calls).</summary>
    public static HttpClient CreateAnonymous(this HttpMessageHandler handler, string baseAddress)
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri(baseAddress) };
        return client.AsAnonymous();
    }
}
