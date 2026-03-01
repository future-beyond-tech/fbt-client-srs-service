using FluentAssertions;
using SRS.Application.Constants;
using SRS.Application.DTOs;
using SRS.Infrastructure.PdfTemplate;
using Xunit;

namespace SRS.UnitTests.Pdf;

/// <summary>
/// Unit tests for Delivery Note HTML template: structure, mandatory Tamil terms, payment checkboxes.
/// </summary>
public sealed class DeliveryNoteHtmlBuilderTests
{
    private static DeliveryNoteTemplateViewModel MinimalVm()
    {
        return new DeliveryNoteTemplateViewModel
        {
            ShopName = "Test Shop",
            ShopAddress = "Address",
            BillNumber = 1,
            BillDate = "01-01-2026",
            TitleLine1 = "DELIVERY NOTE",
            TitleLine2 = "Subtitle",
            SellerLabel = "SELLER",
            BuyerLabel = "BUYER",
            SellerName = "S",
            SellerAddress = "A",
            BuyerName = "B",
            BuyerAddress = "A",
            GreetingLine = "Sir / Madam,",
            RefText = "Ref",
            BodyParagraph = "Body",
            DetailsLeftTitle = "VEHICLE DETAILS",
            DetailsRightTitle = "PAYMENT DETAILS",
            DetailsLeftRows = [],
            UsePaymentCheckboxes = true,
            TamilTerms = "",
            FooterThankYou = "Thank you.",
            SignatureLineLabel = "Authorized",
        };
    }

    [Fact]
    public void Build_ContainsMandatoryTamilTerms_Always()
    {
        var vm = MinimalVm();
        vm.TamilTerms = ""; // Builder ignores vm.TamilTerms for mandatory block; uses PdfContentConstants.TamilTerms

        var html = DeliveryNoteHtmlBuilder.Build(vm, null, null);

        html.Should().Contain("tamil-terms");
        html.Should().Contain("tamil-term");
        html.Should().Contain("☑");
        foreach (var line in PdfContentConstants.TamilTerms)
            html.Should().Contain(line, "each mandatory Tamil line must appear in HTML");
        html.Should().Contain("வண்டி"); // appears in multiple lines
    }

    [Fact]
    public void Build_ContainsTamilTerms_WhenTamilTermsSet()
    {
        var vm = MinimalVm();
        vm.TamilTerms = PdfContentConstants.DefaultTamilTerms;

        var html = DeliveryNoteHtmlBuilder.Build(vm, null, null);

        html.Should().Contain("tamil-terms");
        html.Should().Contain("tamil-term");
        html.Should().Contain("☑");
        html.Should().Contain("வண்டி");
    }

    [Fact]
    public void Build_ContainsPaymentCheckboxes_WhenUsePaymentCheckboxesTrue()
    {
        var vm = MinimalVm();
        vm.UsePaymentCheckboxes = true;
        vm.PaymentCashChecked = true;
        vm.PaymentFinanceChecked = true;
        vm.FinanceName = "HDFC";

        var html = DeliveryNoteHtmlBuilder.Build(vm, null, null);

        html.Should().Contain("payment-checkboxes");
        html.Should().Contain("☑");
        html.Should().Contain("Finance Name:");
        html.Should().Contain("HDFC");
    }

    [Fact]
    public void Build_ContainsHeaderAndCards()
    {
        var vm = MinimalVm();
        vm.ShopName = "My Shop";
        vm.SellerLabel = "SELLER";
        vm.BuyerLabel = "BUYER";

        var html = DeliveryNoteHtmlBuilder.Build(vm, null, null);

        html.Should().Contain("header-bar");
        html.Should().Contain("My Shop");
        html.Should().Contain("SELLER");
        html.Should().Contain("BUYER");
    }

    [Fact]
    public void Build_WhenFontBase64Provided_IncludesFontFace()
    {
        var vm = MinimalVm();
        var fakeBase64 = "Zm9udA=="; // "font" in base64

        var html = DeliveryNoteHtmlBuilder.Build(vm, null, fakeBase64);

        html.Should().Contain("@font-face");
        html.Should().Contain("Noto Sans Tamil");
        html.Should().Contain("data:font/ttf;base64,Zm9udA==");
    }
}
