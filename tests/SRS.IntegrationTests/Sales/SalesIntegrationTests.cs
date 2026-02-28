using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SRS.Tests.Shared;
using Xunit;

namespace SRS.IntegrationTests.Sales;

[Collection(PostgresCollection.Name)]
public sealed class SalesIntegrationTests : IntegrationTestBase
{
    public SalesIntegrationTests(PostgresFixture postgres) : base(postgres)
    {
    }

    [Fact]
    public async Task GetHistory_AsAdmin_Returns200()
    {
        var response = await Client.GetAsync("/api/sales?pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SalesHistoryResponse>();
        body.Should().NotBeNull();
        body!.Items.Should().NotBeNull();
        body.PageNumber.Should().Be(1);
        body.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetHistory_AsAnonymous_Returns401()
    {
        Client.AsAnonymous();
        var response = await Client.GetAsync("/api/sales");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetByBill_AsAdmin_NotFound_Returns404()
    {
        var response = await Client.GetAsync("/api/sales/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed class SalesHistoryResponse
    {
        public object[] Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
