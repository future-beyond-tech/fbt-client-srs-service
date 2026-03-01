using System.Text;
using FluentAssertions;
using SRS.Application.Constants;
using SRS.Application.DTOs;
using SRS.Infrastructure.PdfTemplate;
using Xunit;

namespace SRS.UnitTests.Pdf;

/// <summary>
/// Unit tests for Delivery Note template: HTML output and structure. Replaces QuestPDF DeliveryNotePdfTemplateTests.
/// PDF bytes validation is in integration tests (GET .../pdf?download=true).
/// </summary>
public sealed class DeliveryNoteTemplateTests
{
    private static DeliveryNoteTemplateViewModel MinimalViewModel()
    {
        return new DeliveryNoteTemplateViewModel
        {
            ShopName = "SREE RAMALINGAM SONS",
            ShopAddress = "H.O.: 154, Pycrofts Road, Royapettah (Opp. Sub Reg. Office) Chennai - 600 014",
            BillNumber = 9,
            BillDate = "24-02-2026",
            TitleLine1 = "DELIVERY NOTE",
            TitleLine2 = "Only on Commission Basis",
            TitleLine3 = "SREE RAMALINGAM SONS",
            SellerLabel = "SELLER",
            SellerName = "SREE RAMALINGAM SONS",
            SellerAddress = "H.O.: 154, Pycrofts Road, Royapettah (Opp. Sub Reg. Office) Chennai - 600 014",
            BuyerLabel = "BUYER",
            BuyerName = "Test Buyer",
            BuyerAddress = "Test Address",
            BuyerPhone = "—",
            GreetingLine = "Sir / Madam,",
            RefText = "Ref / Test",
            BodyParagraph = "I have this day purchased the above vehicle from you...",
            RiskParagraph = "The entire risk is being borne by me / us from this time 24-02-2026.",
            DetailsLeftTitle = "VEHICLE DETAILS",
            DetailsLeftRows =
            [
                new DetailRow("Make / Model", "Suzuki"),
                new DetailRow("Chassis No", "C123"),
                new DetailRow("Engine No", "E456"),
            ],
            DetailsRightTitle = "PAYMENT DETAILS",
            DetailsRightRows = Array.Empty<DetailRow>(),
            UsePaymentCheckboxes = true,
            PaymentCashChecked = true,
            PaymentUpiChecked = false,
            PaymentFinanceChecked = true,
            FinanceName = "HDFC Bank",
            TamilTerms = PdfContentConstants.DefaultTamilTerms,
            FooterThankYou = "Thank you for your purchase.",
            SignatureLineLabel = "Authorized Signature",
        };
    }

    [Fact]
    public void BuildHtml_ContainsHeaderAndSellerBuyerCards()
    {
        var vm = MinimalViewModel();
        var html = DeliveryNoteHtmlBuilder.Build(vm, null, null);

        html.Should().Contain("header-bar");
        html.Should().Contain("SREE RAMALINGAM SONS");
        html.Should().Contain("SELLER");
        html.Should().Contain("BUYER");
        html.Should().Contain("Test Buyer");
    }

    [Fact]
    public void BuildHtml_ContainsRefStripAndGreeting()
    {
        var vm = MinimalViewModel();
        var html = DeliveryNoteHtmlBuilder.Build(vm, null, null);

        html.Should().Contain("ref-strip");
        html.Should().Contain("Ref / Test");
        html.Should().Contain("Sir / Madam,");
    }

    [Fact]
    public void BuildHtml_ContainsPaymentCheckboxesAndFinanceName()
    {
        var vm = MinimalViewModel();
        var html = DeliveryNoteHtmlBuilder.Build(vm, null, null);

        html.Should().Contain("payment-checkboxes");
        html.Should().Contain("☑");
        html.Should().Contain("Finance Name:");
        html.Should().Contain("HDFC Bank");
    }

    [Fact]
    public void BuildHtml_ContainsTamilTermsBlock()
    {
        var vm = MinimalViewModel();
        var html = DeliveryNoteHtmlBuilder.Build(vm, null, null);

        html.Should().Contain("tamil-terms");
        html.Should().Contain("tamil-term");
        html.Should().Contain("☑");
        html.Should().Contain("வண்டி");
        foreach (var line in PdfContentConstants.TamilTerms)
            html.Should().Contain(line);
    }

    [Fact]
    public void BuildHtml_ContainsVehicleDetailsTable()
    {
        var vm = MinimalViewModel();
        var html = DeliveryNoteHtmlBuilder.Build(vm, null, null);

        html.Should().Contain("VEHICLE DETAILS");
        html.Should().Contain("details-table");
        html.Should().Contain("Chassis No");
        html.Should().Contain("C123");
    }
}
