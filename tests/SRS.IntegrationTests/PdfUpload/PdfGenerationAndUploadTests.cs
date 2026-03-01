using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SRS.Infrastructure.Persistence;
using SRS.Tests.Shared;
using Xunit;

namespace SRS.IntegrationTests.PdfUpload;

/// <summary>
/// Validates that the backend generates valid PDF bytes and invokes the cloud uploader (ICloudStorageService)
/// with correct arguments. Uses a fake uploader so no real Cloudinary calls in CI; no secrets.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class PdfGenerationAndUploadTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly FakeCloudStorageService _fakeStorage;

    public PdfGenerationAndUploadTests(PostgresFixture postgres)
    {
        _fakeStorage = new FakeCloudStorageService();
        _factory = new TestWebApplicationFactory(postgres.ConnectionString, _fakeStorage);
        _client = _factory.CreateClient();
        _client.AsAdmin();
    }

    public void Dispose() => _factory?.Dispose();

    [Fact]
    public async Task GenerateSalesPdf_ReturnsValidPdfBytes()
    {
        _fakeStorage.Reset();
        int billNumber = await SeedSaleAsync();

        var response = await _client.GetAsync($"/api/sales/{billNumber}/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(4);
        var header = Encoding.ASCII.GetString(bytes.AsSpan(0, 4));
        header.Should().Be("%PDF", "backend must return valid PDF bytes");
    }

    [Fact]
    public async Task SendSalesInvoice_UploadsPdf_ReturnsPdfUrl()
    {
        _fakeStorage.Reset();
        int billNumber = await SeedSaleAsync();

        var response = await _client.PostAsync($"/api/sales/{billNumber}/send-invoice", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SendInvoiceResponse>();
        body.Should().NotBeNull();
        body!.PdfUrl.Should().NotBeNullOrWhiteSpace();
        body.Status.Should().NotBeNullOrWhiteSpace();

        _fakeStorage.UploadCalls.Should().HaveCount(1, "uploader must be invoked once");
        var call = _fakeStorage.UploadCalls[0];
        call.FileNameContains(billNumber.ToString()).Should().BeTrue("fileName must contain bill number");
        call.BytesStartWithPdfHeader.Should().BeTrue("uploaded bytes must be valid PDF");
        body.PdfUrl.Should().StartWith("https://cdn.test/", "API must return the URL from the uploader");
        body.PdfUrl.Should().Contain(billNumber.ToString());
    }

    [Fact]
    public async Task ManualBill_SendInvoice_UploadsPdf_ReturnsPdfUrl()
    {
        _fakeStorage.Reset();
        var createDto = new
        {
            customerName = "Upload Test Customer",
            phone = "+919876543210",
            address = (string?)null,
            photoUrl = "https://example.com/photo.jpg",
            itemDescription = "Item for upload test",
            amountTotal = 100m,
            paymentMode = 1,
            cashAmount = 100m,
            upiAmount = (decimal?)null,
            financeAmount = (decimal?)null,
            financeCompany = (string?)null
        };
        var createRes = await _client.PostAsJsonAsync("/api/manual-bills", createDto);
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<CreateManualBillResponse>();
        created.Should().NotBeNull();
        var billNumber = created!.BillNumber;

        var response = await _client.PostAsync($"/api/manual-bills/{billNumber}/send-invoice", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SendInvoiceResponse>();
        body.Should().NotBeNull();
        body!.PdfUrl.Should().NotBeNullOrWhiteSpace();
        body.Status.Should().NotBeNullOrWhiteSpace();

        _fakeStorage.UploadCalls.Should().HaveCount(1, "uploader must be invoked once");
        var call = _fakeStorage.UploadCalls[0];
        call.FileNameContains("manual-invoice").Should().BeTrue("fileName must indicate manual invoice");
        call.FileNameContains(billNumber.ToString()).Should().BeTrue("fileName must contain bill number");
        call.BytesStartWithPdfHeader.Should().BeTrue("uploaded bytes must be valid PDF");
        body.PdfUrl.Should().StartWith("https://cdn.test/", "API must return the URL from the uploader");
    }

    private async Task<int> SeedSaleAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customer = await TestDataSeeder.EnsureTestCustomerAsync(db, "+919876543210", CancellationToken.None);
        customer.PhotoUrl = "https://example.com/photo.jpg";
        await db.SaveChangesAsync(CancellationToken.None);
        var vehicle = await TestDataSeeder.EnsureTestVehicleAsync(db, "UPL-" + Guid.NewGuid().ToString("N")[..8], 100_000m, ct: CancellationToken.None);
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
        vehicle.Status = SRS.Domain.Enums.VehicleStatus.Sold;
        await db.SaveChangesAsync(CancellationToken.None);
        return sale.BillNumber;
    }

    private sealed class SendInvoiceResponse
    {
        public int BillNumber { get; set; }
        public string PdfUrl { get; set; } = "";
        public string Status { get; set; } = "";
    }

    private sealed class CreateManualBillResponse
    {
        public int BillNumber { get; set; }
        public string? PdfUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
