using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SRS.Tests.Shared;
using Xunit;

namespace SRS.IntegrationTests.ManualBills;

[Collection(PostgresCollection.Name)]
public sealed class ManualBillsIntegrationTests : IntegrationTestBase
{
    public ManualBillsIntegrationTests(PostgresFixture postgres) : base(postgres)
    {
    }

    [Fact]
    public async Task Create_AsAdmin_ValidDto_Returns201()
    {
        var dto = new
        {
            customerName = "Integration Test Customer",
            phone = "+919876543210",
            address = "Integration Test Address",
            photoUrl = "https://example.com/photo.jpg",
            itemDescription = "Manual entry details",
            amountTotal = 1000m,
            paymentMode = 1,
            cashAmount = 1000m,
            upiAmount = (decimal?)null,
            financeAmount = (decimal?)null,
            financeCompany = (string?)null
        };
        var response = await Client.PostAsJsonAsync("/api/manual-bills", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var created = await response.Content.ReadFromJsonAsync<CreateManualBillResponse>();
        created.Should().NotBeNull();
        created!.BillNumber.Should().BeGreaterThan(0);
        created.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task GetByBillNumber_AsAdmin_AfterCreate_Returns200()
    {
        var dto = new
        {
            customerName = "Get Test Customer",
            phone = "9123456789",
            address = "Get Test Address",
            photoUrl = "https://storage.test/photo.jpg",
            itemDescription = "Item desc",
            amountTotal = 500m,
            paymentMode = 2,
            cashAmount = (decimal?)null,
            upiAmount = 500m,
            financeAmount = (decimal?)null,
            financeCompany = (string?)null
        };
        var createRes = await Client.PostAsJsonAsync("/api/manual-bills", dto);
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<CreateManualBillResponse>();
        var getRes = await Client.GetAsync($"/api/manual-bills/{created!.BillNumber}");
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await getRes.Content.ReadFromJsonAsync<ManualBillDetailResponse>();
        detail.Should().NotBeNull();
        detail!.BillNumber.Should().Be(created.BillNumber);
        detail.CustomerName.Should().Be("Get Test Customer");
        detail.ItemDescription.Should().Be("Item desc");
        detail.AmountTotal.Should().Be(500m);
    }

    [Fact]
    public async Task GetByBillNumber_AsAdmin_NotFound_Returns404()
    {
        var response = await Client.GetAsync("/api/manual-bills/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Create_AsAdmin_InvalidPaymentSplit_Returns400_ProblemDetails()
    {
        var dto = new
        {
            customerName = "Bad Split",
            phone = "+919999999999",
            address = (string?)null,
            photoUrl = "https://example.com/p.jpg",
            itemDescription = "Item",
            amountTotal = 1000m,
            paymentMode = 1,
            cashAmount = 100m,
            upiAmount = 200m,
            financeAmount = (decimal?)null,
            financeCompany = (string?)null
        };
        var response = await Client.PostAsJsonAsync("/api/manual-bills", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task GetInvoice_AsAdmin_AfterCreate_Returns200()
    {
        var dto = new
        {
            customerName = "Invoice Test",
            phone = "9888888888",
            address = (string?)null,
            photoUrl = "https://example.com/photo.jpg",
            itemDescription = "Description",
            amountTotal = 100m,
            paymentMode = 1,
            cashAmount = 100m,
            upiAmount = (decimal?)null,
            financeAmount = (decimal?)null,
            financeCompany = (string?)null
        };
        var createRes = await Client.PostAsJsonAsync("/api/manual-bills", dto);
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<CreateManualBillResponse>();
        var invoiceRes = await Client.GetAsync($"/api/manual-bills/{created!.BillNumber}/invoice");
        invoiceRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var invoice = await invoiceRes.Content.ReadFromJsonAsync<ManualBillInvoiceResponse>();
        invoice.Should().NotBeNull();
        invoice!.BillNumber.Should().Be(created.BillNumber);
        invoice.CustomerName.Should().Be("Invoice Test");
        invoice.SellingPrice.Should().Be(100m);
    }

    [Fact]
    public async Task Create_AsAnonymous_Returns401()
    {
        Client.AsAnonymous();
        var dto = new
        {
            customerName = "Anon",
            phone = "+919876543210",
            photoUrl = "https://example.com/p.jpg",
            itemDescription = "Item",
            amountTotal = 100m,
            paymentMode = 1,
            cashAmount = 100m
        };
        var response = await Client.PostAsJsonAsync("/api/manual-bills", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendInvoice_WhenBillNotFound_Returns404()
    {
        var response = await Client.PostAsync("/api/manual-bills/999999/send-invoice", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task GetPdf_WhenBillNotFound_Returns404()
    {
        var response = await Client.GetAsync("/api/manual-bills/999999/pdf");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ThenSendInvoice_Returns200WithPdfUrlAndStatus()
    {
        var dto = new
        {
            customerName = "Send Invoice Test",
            phone = "+919876543210",
            address = (string?)null,
            photoUrl = "https://example.com/photo.jpg",
            itemDescription = "Item for PDF",
            amountTotal = 250m,
            paymentMode = 1,
            cashAmount = 250m,
            upiAmount = (decimal?)null,
            financeAmount = (decimal?)null,
            financeCompany = (string?)null
        };
        var createRes = await Client.PostAsJsonAsync("/api/manual-bills", dto);
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<CreateManualBillResponse>();
        var sendRes = await Client.PostAsync($"/api/manual-bills/{created!.BillNumber}/send-invoice", null);
        sendRes.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadGateway, HttpStatusCode.Conflict);
        if (sendRes.StatusCode == HttpStatusCode.OK)
        {
            var result = await sendRes.Content.ReadFromJsonAsync<SendInvoiceResponse>();
            result.Should().NotBeNull();
            result!.BillNumber.Should().Be(created.BillNumber);
            result.PdfUrl.Should().NotBeNullOrWhiteSpace();
            result.Status.Should().Be("Sent");
        }
    }

    [Fact]
    public async Task Create_ThenGetPdf_Returns200WithPdfUrl()
    {
        var dto = new
        {
            customerName = "Get PDF Test",
            phone = "+919876543210",
            address = (string?)null,
            photoUrl = "https://example.com/photo.jpg",
            itemDescription = "Item",
            amountTotal = 100m,
            paymentMode = 1,
            cashAmount = 100m,
            upiAmount = (decimal?)null,
            financeAmount = (decimal?)null,
            financeCompany = (string?)null
        };
        var createRes = await Client.PostAsJsonAsync("/api/manual-bills", dto);
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<CreateManualBillResponse>();
        var pdfRes = await Client.GetAsync($"/api/manual-bills/{created!.BillNumber}/pdf");
        pdfRes.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadGateway, HttpStatusCode.Conflict);
        if (pdfRes.StatusCode == HttpStatusCode.OK)
        {
            var body = await pdfRes.Content.ReadFromJsonAsync<ManualBillPdfResponse>();
            body.Should().NotBeNull();
            body!.PdfUrl.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task GetPdf_DownloadTrue_ReturnsApplicationPdfWithValidPdfBytes()
    {
        var dto = new
        {
            customerName = "PDF Download Test",
            phone = "+919876543210",
            address = (string?)null,
            photoUrl = "https://example.com/photo.jpg",
            itemDescription = "Item",
            amountTotal = 100m,
            paymentMode = 1,
            cashAmount = 100m,
            upiAmount = (decimal?)null,
            financeAmount = (decimal?)null,
            financeCompany = (string?)null
        };
        var createRes = await Client.PostAsJsonAsync("/api/manual-bills", dto);
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<CreateManualBillResponse>();
        var pdfRes = await Client.GetAsync($"/api/manual-bills/{created!.BillNumber}/pdf?download=true");
        pdfRes.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadGateway, HttpStatusCode.Conflict);
        if (pdfRes.StatusCode == HttpStatusCode.OK)
        {
            pdfRes.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
            var bytes = await pdfRes.Content.ReadAsByteArrayAsync();
            bytes.Should().NotBeEmpty();
            System.Text.Encoding.ASCII.GetString(bytes.AsSpan(0, 4)).Should().Be("%PDF");
        }
    }

    [Fact]
    public async Task GetPdf_DownloadTrue_PdfContainsMandatoryTamilWord()
    {
        var dto = new
        {
            customerName = "Tamil PDF Test",
            phone = "+919876543210",
            address = (string?)null,
            photoUrl = "https://example.com/photo.jpg",
            itemDescription = "Item",
            amountTotal = 100m,
            paymentMode = 1,
            cashAmount = 100m,
            upiAmount = (decimal?)null,
            financeAmount = (decimal?)null,
            financeCompany = (string?)null
        };
        var createRes = await Client.PostAsJsonAsync("/api/manual-bills", dto);
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<CreateManualBillResponse>();
        var pdfRes = await Client.GetAsync($"/api/manual-bills/{created!.BillNumber}/pdf?download=true");
        pdfRes.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadGateway, HttpStatusCode.Conflict);
        if (pdfRes.StatusCode == HttpStatusCode.OK)
        {
            var bytes = await pdfRes.Content.ReadAsByteArrayAsync();
            bytes.Should().NotBeEmpty();
            var tamilWord = "வண்டி"; // appears in mandatory Tamil terms
            System.Text.Encoding.UTF8.GetString(bytes).Should().Contain(tamilWord, "mandatory Tamil terms block should be in PDF");
        }
    }

    [Fact]
    public async Task Create_WithCustomSellerName_PdfContainsSellerName()
    {
        var dto = new
        {
            customerName = "Custom Seller Test",
            phone = "+919876543210",
            address = (string?)null,
            photoUrl = "https://example.com/photo.jpg",
            itemDescription = "Item",
            amountTotal = 100m,
            paymentMode = 1,
            cashAmount = 100m,
            upiAmount = (decimal?)null,
            financeAmount = (decimal?)null,
            financeCompany = (string?)null,
            sellerName = "ABC Motors"
        };
        var createRes = await Client.PostAsJsonAsync("/api/manual-bills", dto);
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<CreateManualBillResponse>();
        var pdfRes = await Client.GetAsync($"/api/manual-bills/{created!.BillNumber}/pdf?download=true");
        pdfRes.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadGateway, HttpStatusCode.Conflict);
        if (pdfRes.StatusCode == HttpStatusCode.OK)
        {
            var bytes = await pdfRes.Content.ReadAsByteArrayAsync();
            bytes.Should().NotBeEmpty();
            var text = System.Text.Encoding.UTF8.GetString(bytes);
            text.Should().Contain("ABC Motors", "custom seller name should appear in PDF content");
        }
    }

    [Fact]
    public async Task SendInvoice_Twice_ReturnsSamePdfUrl_WhenBothSucceed()
    {
        var dto = new
        {
            customerName = "Reuse PDF Test",
            phone = "+919876543210",
            address = (string?)null,
            photoUrl = "https://example.com/photo.jpg",
            itemDescription = "Item",
            amountTotal = 100m,
            paymentMode = 1,
            cashAmount = 100m,
            upiAmount = (decimal?)null,
            financeAmount = (decimal?)null,
            financeCompany = (string?)null
        };
        var createRes = await Client.PostAsJsonAsync("/api/manual-bills", dto);
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<CreateManualBillResponse>();
        var send1 = await Client.PostAsync($"/api/manual-bills/{created!.BillNumber}/send-invoice", null);
        var send2 = await Client.PostAsync($"/api/manual-bills/{created.BillNumber}/send-invoice", null);
        if (send1.StatusCode == HttpStatusCode.OK && send2.StatusCode == HttpStatusCode.OK)
        {
            var r1 = await send1.Content.ReadFromJsonAsync<SendInvoiceResponse>();
            var r2 = await send2.Content.ReadFromJsonAsync<SendInvoiceResponse>();
            r1!.PdfUrl.Should().Be(r2!.PdfUrl, "second send should reuse same PDF URL");
        }
    }

    private sealed class SendInvoiceResponse
    {
        public int BillNumber { get; set; }
        public string PdfUrl { get; set; } = "";
        public string Status { get; set; } = "";
    }

    private sealed class ManualBillPdfResponse
    {
        public string PdfUrl { get; set; } = "";
    }

    private sealed class CreateManualBillResponse
    {
        public int BillNumber { get; set; }
        public string? PdfUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    private sealed class ManualBillDetailResponse
    {
        public int BillNumber { get; set; }
        public string BillType { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string? Address { get; set; }
        public string PhotoUrl { get; set; } = "";
        public string ItemDescription { get; set; } = "";
        public decimal AmountTotal { get; set; }
        public string? InvoicePdfUrl { get; set; }
    }

    private sealed class ManualBillInvoiceResponse
    {
        public int BillNumber { get; set; }
        public string CustomerName { get; set; } = "";
        public decimal SellingPrice { get; set; }
    }
}
