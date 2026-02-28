using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SRS.Tests.Shared;
using Xunit;

namespace SRS.IntegrationTests.Customers;

[Collection(PostgresCollection.Name)]
public sealed class CustomersIntegrationTests : IntegrationTestBase
{
    public CustomersIntegrationTests(PostgresFixture postgres) : base(postgres)
    {
    }

    [Fact]
    public async Task GetAll_AsAdmin_Returns200()
    {
        var response = await Client.GetAsync("/api/customers");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<object[]>();
        items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_AsAnonymous_Returns401()
    {
        Client.AsAnonymous();
        var response = await Client.GetAsync("/api/customers");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsAdmin_ValidDto_Returns201()
    {
        var dto = new
        {
            name = "Integration Test Customer",
            phone = "8888888888",
            address = (string?)null
        };
        var response = await Client.PostAsJsonAsync("/api/customers", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var created = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Name.Should().Be("Integration Test Customer");
        created.Phone.Should().Be("8888888888");
    }

    [Fact]
    public async Task Create_AsAdmin_DuplicatePhone_Returns400Or201()
    {
        var dto = new { name = "First", phone = "7777777777", address = (string?)null };
        await Client.PostAsJsonAsync("/api/customers", dto);
        var second = await Client.PostAsJsonAsync("/api/customers", dto);
        second.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetById_AsAdmin_Existing_Returns200()
    {
        var create = new { name = "For GetById", phone = "6666666666", address = (string?)null };
        var createRes = await Client.PostAsJsonAsync("/api/customers", create);
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<CustomerResponse>();
        var getRes = await Client.GetAsync($"/api/customers/{created!.Id}");
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_AsAdmin_NotFound_Returns404()
    {
        var getRes = await Client.GetAsync($"/api/customers/{Guid.NewGuid()}");
        getRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Search_AsAdmin_Returns200()
    {
        var response = await Client.GetAsync("/api/customers/search?phone=888");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<object[]>();
        items.Should().NotBeNull();
    }

    private sealed class CustomerResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
    }
}
