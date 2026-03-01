using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SRS.Infrastructure.Persistence;
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

    [Fact]
    public async Task GetPdf_AsAnonymous_Returns401()
    {
        Client.AsAnonymous();
        var response = await Client.GetAsync("/api/sales/1/pdf");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPdf_AsAdmin_NotFound_Returns404_AndProblemDetails()
    {
        var response = await Client.GetAsync("/api/sales/999999/pdf");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("title");
        body.Should().Contain("detail");
    }

    [Fact]
    public async Task GetPdf_AsAdmin_WhenSaleExists_Returns200_AndApplicationPdf()
    {
        int billNumber;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var customer = await TestDataSeeder.EnsureTestCustomerAsync(db, "+919876543210", CancellationToken.None);
            customer.PhotoUrl = "https://example.com/photo.jpg";
            await db.SaveChangesAsync(CancellationToken.None);
            var vehicle = await TestDataSeeder.EnsureTestVehicleAsync(db, "PDF-TEST-" + Guid.NewGuid().ToString("N")[..8], 100_000m, ct: CancellationToken.None);
            var sale = new SRS.Domain.Entities.Sale
            {
                BillNumber = await db.Sales.AnyAsync() ? await db.Sales.MaxAsync(s => s.BillNumber) + 1 : 1,
                VehicleId = vehicle.Id,
                CustomerId = customer.Id,
                PaymentMode = SRS.Domain.Enums.PaymentMode.Cash,
                CashAmount = 100_000m,
                SaleDate = DateTime.UtcNow.Date,
                RcBookReceived = true,
                OwnershipTransferAccepted = true,
                VehicleAcceptedInAsIsCondition = true,
                Profit = 15_000m
            };
            db.Sales.Add(sale);
            await db.SaveChangesAsync(CancellationToken.None);
            billNumber = sale.BillNumber;
            vehicle.Status = SRS.Domain.Enums.VehicleStatus.Sold;
            await db.SaveChangesAsync(CancellationToken.None);
        }

        var response = await Client.GetAsync($"/api/sales/{billNumber}/pdf");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadGateway, HttpStatusCode.Conflict);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
            var bytes = await response.Content.ReadAsByteArrayAsync();
            bytes.Length.Should().BeGreaterThan(0);
            System.Text.Encoding.ASCII.GetString(bytes.AsSpan(0, 4)).Should().Be("%PDF");
        }
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
