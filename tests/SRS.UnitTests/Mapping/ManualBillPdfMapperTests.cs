using FluentAssertions;
using SRS.Application.DTOs;
using SRS.Application.Mapping;
using Xunit;

namespace SRS.UnitTests.Mapping;

public sealed class ManualBillPdfMapperTests
{
    private static DeliveryNoteSettingsDto Settings() => new()
    {
        Id = 1,
        ShopName = "Test Shop",
        ShopAddress = "123 Street",
        FooterText = "Thank you.",
        TermsAndConditions = "",
        SignatureLine = "Authorized Signature",
        UpdatedAt = DateTime.UtcNow
    };

    private static ManualBillPdfViewModel ViewModel(
        string? itemDescription = "Item",
        string? chassisNo = "C1",
        string? engineNo = "E1",
        string? color = "Red",
        string? notes = "OK",
        bool financeChecked = false,
        string? financeCompany = null)
    {
        return new ManualBillPdfViewModel
        {
            BillNumber = 9,
            SaleDate = new DateTime(2026, 2, 24, 10, 52, 0),
            SellerName = "Test Shop",
            SellerAddress = "123 Street",
            BuyerName = "Buyer",
            BuyerAddress = "Addr",
            BuyerPhone = "123",
            GreetingLine = "Dear Buyer,", // mapper overrides to Sir / Madam
            ItemDescription = itemDescription ?? "",
            ChassisNo = chassisNo ?? "",
            EngineNo = engineNo ?? "",
            Color = color ?? "",
            Notes = notes ?? "",
            CashChecked = true,
            UpiChecked = false,
            FinanceChecked = financeChecked,
            FinanceCompanyDisplay = financeCompany ?? "-",
            TotalAmountFormatted = "Rs. 1,20,000",
            AmountInWords = "One Lakh Twenty Thousand Rupees only"
        };
    }

    [Fact]
    public void ToTemplateViewModel_EmptyOrNullVehicleFields_MapsToDash()
    {
        var viewModel = ViewModel(itemDescription: null, chassisNo: "", engineNo: null, color: "  ", notes: null);
        var settings = Settings();

        var result = ManualBillPdfMapper.ToTemplateViewModel(viewModel, settings);

        result.DetailsLeftTitle.Should().Be("VEHICLE DETAILS");
        result.DetailsLeftRows.Should().HaveCount(5);
        result.DetailsLeftRows[0].Value.Should().Be("-");
        result.DetailsLeftRows[1].Value.Should().Be("-");
        result.DetailsLeftRows[2].Value.Should().Be("-");
        result.DetailsLeftRows[3].Value.Should().Be("-");
        result.DetailsLeftRows[4].Value.Should().Be("-");
    }

    [Fact]
    public void ToTemplateViewModel_FinanceSelected_FinanceNameSetInTemplateViewModel()
    {
        var viewModel = ViewModel(financeChecked: true, financeCompany: "HDFC Bank");
        var settings = Settings();

        var result = ManualBillPdfMapper.ToTemplateViewModel(viewModel, settings);

        result.UsePaymentCheckboxes.Should().BeTrue();
        result.PaymentFinanceChecked.Should().BeTrue();
        result.FinanceName.Should().Be("HDFC Bank");
    }

    [Fact]
    public void ToTemplateViewModel_FinanceNotSelected_FinanceNameIsDash()
    {
        var viewModel = ViewModel(financeChecked: false, financeCompany: "HDFC Bank");
        var settings = Settings();

        var result = ManualBillPdfMapper.ToTemplateViewModel(viewModel, settings);

        result.FinanceName.Should().Be("-");
    }

    [Fact]
    public void ToTemplateViewModel_GreetingLine_PassesThroughFromViewModel()
    {
        var viewModel = ViewModel();
        viewModel.GreetingLine = "Dear John,";
        var settings = Settings();

        var result = ManualBillPdfMapper.ToTemplateViewModel(viewModel, settings);

        result.GreetingLine.Should().Be("Dear John,");
    }

    [Fact]
    public void ToTemplateViewModel_GreetingLine_WhenNullOrEmpty_FallsBackToSirMadam()
    {
        var viewModel = ViewModel();
        viewModel.GreetingLine = null;
        var settings = Settings();

        var result = ManualBillPdfMapper.ToTemplateViewModel(viewModel, settings);

        result.GreetingLine.Should().Be("Sir / Madam,");
    }

    [Fact]
    public void ToTemplateViewModel_SellerBuyerLabels_AreSellerAndBuyer()
    {
        var viewModel = ViewModel();
        var settings = Settings();

        var result = ManualBillPdfMapper.ToTemplateViewModel(viewModel, settings);

        result.SellerLabel.Should().Be("SELLER");
        result.BuyerLabel.Should().Be("BUYER");
    }

    [Fact]
    public void ToTemplateViewModel_DetailsLeftTitle_IsVehicleDetails()
    {
        var viewModel = ViewModel();
        var settings = Settings();

        var result = ManualBillPdfMapper.ToTemplateViewModel(viewModel, settings);

        result.DetailsLeftTitle.Should().Be("VEHICLE DETAILS");
        result.DetailsLeftRows[0].Label.Should().Be("Make / Model");
    }

    [Fact]
    public void ToTemplateViewModel_UsePaymentCheckboxes_True()
    {
        var viewModel = ViewModel();
        var settings = Settings();

        var result = ManualBillPdfMapper.ToTemplateViewModel(viewModel, settings);

        result.UsePaymentCheckboxes.Should().BeTrue();
        result.PaymentCashChecked.Should().BeTrue();
        result.PaymentUpiChecked.Should().BeFalse();
        result.PaymentFinanceChecked.Should().BeFalse();
    }

    [Fact]
    public void ToTemplateViewModel_NullViewModel_Throws()
    {
        var act = () => ManualBillPdfMapper.ToTemplateViewModel(null!, Settings());
        act.Should().Throw<ArgumentNullException>().WithParameterName("viewModel");
    }

    [Fact]
    public void ToTemplateViewModel_NullSettings_Throws()
    {
        var act = () => ManualBillPdfMapper.ToTemplateViewModel(ViewModel(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("settings");
    }

    [Fact]
    public void ToTemplateViewModel_TamilTermsAndConditions_PreferredOverTermsAndConditions()
    {
        var viewModel = ViewModel();
        var settings = Settings();
        settings.TamilTermsAndConditions = "Tamil line 1\nTamil line 2";
        settings.TermsAndConditions = "English terms";

        var result = ManualBillPdfMapper.ToTemplateViewModel(viewModel, settings);

        result.TamilTerms.Should().Be("Tamil line 1\nTamil line 2");
    }

    [Fact]
    public void ToTemplateViewModel_WhenNoTamilTerms_UsesTermsAndConditions()
    {
        var viewModel = ViewModel();
        var settings = Settings();
        settings.TamilTermsAndConditions = null;
        settings.TermsAndConditions = "English terms text";

        var result = ManualBillPdfMapper.ToTemplateViewModel(viewModel, settings);

        result.TamilTerms.Should().Be("English terms text");
    }

    [Fact]
    public void ToTemplateViewModel_WhenBothEmpty_UsesDefaultTamilTerms()
    {
        var viewModel = ViewModel();
        var settings = Settings();
        settings.TamilTermsAndConditions = null;
        settings.TermsAndConditions = null;

        var result = ManualBillPdfMapper.ToTemplateViewModel(viewModel, settings);

        result.TamilTerms.Should().NotBeNullOrWhiteSpace();
        result.TamilTerms.Should().Contain("☑");
        result.TamilTerms.Should().Contain("வண்டி");
    }
}
