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
/// Verifies that the backend generates PDFs and uploads them to Cloudinary (or configured storage),
/// returning a valid HTTPS URL that serves application/pdf with valid PDF bytes (%PDF header).
/// When Cloudinary is not configured (test env), upload may return 502/409; fetch validation is skipped.
/// Never logs API secrets or unmasked phone numbers.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class PdfUploadVerificationTests : IntegrationTestBase
{
    /// <summary>Cloudinary delivery URL host; optional custom CDN could be configured.</summary>
    private const string CloudinaryHost = "res.cloudinary.com";

    public PdfUploadVerificationTests(PostgresFixture postgres) : base(postgres)
    {
    }

    [Fact]
    [Trait("Category", "CloudStorageSmoke")]
    public async Task Sales_SendInvoice_ReturnsPdfUrl_AndUrlServesValidPdf_WhenUploadSucceeds()
    {
        int billNumber = await SeedSaleAsync();

        var response = await Client.PostAsync($"/api/sales/{billNumber}/send-invoice", null);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadGateway, HttpStatusCode.Conflict);
            return;
        }

        var body = await response.Content.ReadFromJsonAsync<SendInvoiceResponse>();
        body.Should().NotBeNull();
        body!.PdfUrl.Should().NotBeNullOrWhiteSpace();
        body.Status.Should().NotBeNullOrWhiteSpace();

        if (!body.PdfUrl.Contains(CloudinaryHost, StringComparison.OrdinalIgnoreCase))
            return;

        body.PdfUrl.Should().StartWith("https://", "PDF URL must be HTTPS");

        var fetchResponse = await Client.GetAsync(body.PdfUrl);
        fetchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var contentType = fetchResponse.Content.Headers.ContentType?.MediaType ?? "";
        contentType.Should().BeOneOf("application/pdf", "application/octet-stream");
        var bytes = await fetchResponse.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(5);
        var header = Encoding.ASCII.GetString(bytes.AsSpan(0, 4));
        header.Should().Be("%PDF", "Stored file must be a valid PDF");
    }

    [Fact]
    [Trait("Category", "CloudStorageSmoke")]
    public async Task ManualBill_SendInvoice_ReturnsPdfUrl_AndUrlServesValidPdf_WhenUploadSucceeds()
    {
        var createDto = new
        {
            customerName = "PDF Verify Customer",
            phone = "+919876543210",
            address = (string?)null,
            photoUrl = "https://example.com/photo.jpg",
            itemDescription = "Item for PDF verification",
            amountTotal = 100m,
            paymentMode = 1,
            cashAmount = 100m,
            upiAmount = (decimal?)null,
            financeAmount = (decimal?)null,
            financeCompany = (string?)null
        };
        var createRes = await Client.PostAsJsonAsync("/api/manual-bills", createDto);
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<CreateManualBillResponse>();
        created.Should().NotBeNull();
        var billNumber = created!.BillNumber;

        var response = await Client.PostAsync($"/api/manual-bills/{billNumber}/send-invoice", null);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadGateway, HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
            return;
        }

        var body = await response.Content.ReadFromJsonAsync<SendInvoiceResponse>();
        body.Should().NotBeNull();
        body!.PdfUrl.Should().NotBeNullOrWhiteSpace();
        body.Status.Should().NotBeNullOrWhiteSpace();

        if (!body.PdfUrl.Contains(CloudinaryHost, StringComparison.OrdinalIgnoreCase))
            return;

        body.PdfUrl.Should().StartWith("https://", "PDF URL must be HTTPS");

        var fetchResponse = await Client.GetAsync(body.PdfUrl);
        fetchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var contentType = fetchResponse.Content.Headers.ContentType?.MediaType ?? "";
        contentType.Should().BeOneOf("application/pdf", "application/octet-stream");
        var bytes = await fetchResponse.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(5);
        var header = Encoding.ASCII.GetString(bytes.AsSpan(0, 4));
        header.Should().Be("%PDF", "Stored file must be a valid PDF");
    }

    private async Task<int> SeedSaleAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customer = await TestDataSeeder.EnsureTestCustomerAsync(db, "+919876543210", CancellationToken.None);
        customer.PhotoUrl = "https://example.com/photo.jpg";
        await db.SaveChangesAsync(CancellationToken.None);
        var vehicle = await TestDataSeeder.EnsureTestVehicleAsync(db, "UPLOAD-" + Guid.NewGuid().ToString("N")[..8], 100_000m, ct: CancellationToken.None);
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
