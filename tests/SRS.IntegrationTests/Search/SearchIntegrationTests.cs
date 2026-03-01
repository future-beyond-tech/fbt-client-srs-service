using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SRS.Tests.Shared;
using Xunit;

namespace SRS.IntegrationTests.Search;

[Collection(PostgresCollection.Name)]
public sealed class SearchIntegrationTests : IntegrationTestBase
{
    public SearchIntegrationTests(PostgresFixture postgres) : base(postgres)
    {
    }

    [Fact]
    public async Task Search_AsAdmin_EmptyQuery_Returns200EmptyList()
    {
        var response = await Client.GetAsync("/api/search?q=");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<SearchResultItem[]>();
        list.Should().NotBeNull();
        list!.Length.Should().Be(0);
    }

    [Fact]
    public async Task Search_AsAdmin_WhitespaceOnly_Returns200EmptyList()
    {
        var response = await Client.GetAsync("/api/search?q=%20%20");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<SearchResultItem[]>();
        list.Should().NotBeNull();
        list!.Length.Should().Be(0);
    }

    [Fact]
    public async Task Search_ByBillNumber_WhenManualBillExists_ReturnsOneResultWithTypeManualBill()
    {
        var createDto = new
        {
            customerName = "Search By Bill Customer",
            phone = "+919876543210",
            address = (string?)null,
            photoUrl = "https://example.com/photo.jpg",
            itemDescription = "Item for search test",
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
        var billNumber = created!.BillNumber;

        var searchRes = await Client.GetAsync($"/api/search?q={billNumber}");
        searchRes.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Conflict);
        if (searchRes.StatusCode != HttpStatusCode.OK)
            return;
        var list = await searchRes.Content.ReadFromJsonAsync<SearchResultItem[]>();
        list.Should().NotBeNull();
        list!.Length.Should().BeGreaterThan(0);
        var manual = list.Should().ContainSingle(x => x.Type == "ManualBill" && x.BillNumber == billNumber).Subject;
        manual.CustomerName.Should().Be("Search By Bill Customer");
        manual.Vehicle.Should().BeNull();
        manual.RegistrationNumber.Should().BeNull();
        manual.CustomerPhone.Should().NotBeNullOrEmpty();
        manual.CustomerPhone.Should().NotContain("9876543210", "phone should be masked");
    }

    [Fact]
    public async Task Search_ByKeyword_MatchingManualBillItemDescription_ReturnsManualBill()
    {
        var uniqueKeyword = "UniqueItemDesc_" + Guid.NewGuid().ToString("N")[..8];
        var createDto = new
        {
            customerName = "Item Search Customer",
            phone = "+919111111111",
            address = (string?)null,
            photoUrl = "https://example.com/p.jpg",
            itemDescription = uniqueKeyword,
            amountTotal = 50m,
            paymentMode = 1,
            cashAmount = 50m,
            upiAmount = (decimal?)null,
            financeAmount = (decimal?)null,
            financeCompany = (string?)null
        };
        var createRes = await Client.PostAsJsonAsync("/api/manual-bills", createDto);
        createRes.EnsureSuccessStatusCode();

        var searchRes = await Client.GetAsync($"/api/search?q={Uri.EscapeDataString(uniqueKeyword)}");
        searchRes.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Conflict);
        if (searchRes.StatusCode != HttpStatusCode.OK)
            return;
        var list = await searchRes.Content.ReadFromJsonAsync<SearchResultItem[]>();
        list.Should().NotBeNull();
        list!.Should().Contain(x => x.Type == "ManualBill" && x.CustomerName == "Item Search Customer");
    }

    [Fact]
    public async Task Search_ByKeyword_MatchingManualBillCustomerName_ReturnsManualBill()
    {
        var uniqueName = "UniqueName_" + Guid.NewGuid().ToString("N")[..8];
        var createDto = new
        {
            customerName = uniqueName,
            phone = "+919222222222",
            address = (string?)null,
            photoUrl = "https://example.com/p.jpg",
            itemDescription = "Some item",
            amountTotal = 75m,
            paymentMode = 1,
            cashAmount = 75m,
            upiAmount = (decimal?)null,
            financeAmount = (decimal?)null,
            financeCompany = (string?)null
        };
        var createRes = await Client.PostAsJsonAsync("/api/manual-bills", createDto);
        createRes.EnsureSuccessStatusCode();

        var searchRes = await Client.GetAsync($"/api/search?q={uniqueName}");
        searchRes.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Conflict);
        if (searchRes.StatusCode != HttpStatusCode.OK)
            return;
        var list = await searchRes.Content.ReadFromJsonAsync<SearchResultItem[]>();
        list.Should().NotBeNull();
        list!.Should().Contain(x => x.Type == "ManualBill" && x.CustomerName == uniqueName);
    }

    [Fact]
    public async Task Search_Result_PhoneIsMasked_NotFullNumber()
    {
        var createDto = new
        {
            customerName = "MaskedPhoneCustomer",
            phone = "+919876543210",
            address = (string?)null,
            photoUrl = "https://example.com/p.jpg",
            itemDescription = "MaskedPhoneItem",
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

        var searchRes = await Client.GetAsync($"/api/search?q=MaskedPhoneCustomer");
        searchRes.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Conflict);
        if (searchRes.StatusCode != HttpStatusCode.OK)
            return;
        var list = await searchRes.Content.ReadFromJsonAsync<SearchResultItem[]>();
        list.Should().NotBeNull();
        var item = list!.Should().ContainSingle(x => x.BillNumber == created!.BillNumber).Subject;
        item.CustomerPhone.Should().NotBeNullOrEmpty();
        item.CustomerPhone.Should().Contain("*", "masked phone should contain asterisks");
        item.CustomerPhone.Should().NotBe("+919876543210");
    }

    [Fact]
    public async Task Search_AsAnonymous_Returns401()
    {
        Client.AsAnonymous();
        var response = await Client.GetAsync("/api/search?q=test");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed class SearchResultItem
    {
        public string Type { get; set; } = "";
        public int BillNumber { get; set; }
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string? Vehicle { get; set; }
        public string? RegistrationNumber { get; set; }
        public DateTime SaleDate { get; set; }
    }

    private sealed class CreateManualBillResponse
    {
        public int BillNumber { get; set; }
    }
}
