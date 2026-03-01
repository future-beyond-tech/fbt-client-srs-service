using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SRS.Tests.Shared;
using Xunit;

namespace SRS.IntegrationTests.Vehicles;

[Collection(PostgresCollection.Name)]
public sealed class VehiclesIntegrationTests : IntegrationTestBase
{
    public VehiclesIntegrationTests(PostgresFixture postgres) : base(postgres)
    {
    }

    [Fact]
    public async Task GetAll_NoAuthRequired_Returns200()
    {
        Client.AsAnonymous();
        var response = await Client.GetAsync("/api/vehicles");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<object[]>();
        items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAvailable_NoAuthRequired_Returns200()
    {
        Client.AsAnonymous();
        var response = await Client.GetAsync("/api/vehicles/available");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_AsAdmin_NotFound_Returns404()
    {
        var response = await Client.GetAsync("/api/vehicles/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
