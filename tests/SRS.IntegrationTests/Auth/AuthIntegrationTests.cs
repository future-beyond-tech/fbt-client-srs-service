using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SRS.Tests.Shared;
using Xunit;

namespace SRS.IntegrationTests.Auth;

[Collection(PostgresCollection.Name)]
public sealed class AuthIntegrationTests : IntegrationTestBase
{
    public AuthIntegrationTests(PostgresFixture postgres) : base(postgres)
    {
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndToken()
    {
        var request = new { username = "admin", password = "Admin@123" };
        var response = await Client.PostAsJsonAsync("/api/auth/login", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<LoginResponse>();
        json.Should().NotBeNull();
        json!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var request = new { username = "admin", password = "WrongPassword" };
        var response = await Client.PostAsJsonAsync("/api/auth/login", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task Login_WithMissingUsername_Returns401Or400()
    {
        var request = new { username = "", password = "Admin@123" };
        var response = await Client.PostAsJsonAsync("/api/auth/login", request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    private sealed class LoginResponse
    {
        public string Token { get; set; } = "";
    }
}
