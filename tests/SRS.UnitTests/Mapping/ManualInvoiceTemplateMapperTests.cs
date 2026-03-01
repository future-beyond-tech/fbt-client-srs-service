using FluentAssertions;
using SRS.Application.DTOs;
using SRS.Application.Mapping;
using SRS.Domain.Entities;
using SRS.Domain.Enums;
using Xunit;

namespace SRS.UnitTests.Mapping;

public sealed class ManualInvoiceTemplateMapperTests
{
    private static DeliveryNoteSettingsDto Settings() => new()
    {
        Id = 1,
        ShopName = "SREE RAMALINGAM SONS",
        ShopAddress = "123 Main St, City",
        FooterText = "Thank you.",
        TermsAndConditions = "Terms text",
        SignatureLine = "Customer Signature",
        UpdatedAt = DateTime.UtcNow
    };

    [Fact]
    public void ToPdfViewModel_MapsBillAndSettings_WithSellerAndBuyer()
    {
        var bill = new ManualBill
        {
            BillNumber = 42,
            CustomerName = "John Buyer",
            Phone = "+919876543210",
            Address = "Buyer Address",
            ItemDescription = "Widget",
            AmountTotal = 120000m,
            PaymentMode = PaymentMode.Cash,
            CashAmount = 120000m,
            CreatedAtUtc = new DateTime(2025, 2, 28, 10, 0, 0, DateTimeKind.Utc)
        };
        var settings = Settings();

        var result = ManualInvoiceTemplateMapper.ToPdfViewModel(bill, settings);

        result.BillNumber.Should().Be(42);
        result.SaleDate.Should().Be(bill.CreatedAtUtc);
        result.SellerName.Should().Be("SREE RAMALINGAM SONS");
        result.SellerAddress.Should().Be("123 Main St, City");
        result.BuyerName.Should().Be("John Buyer");
        result.BuyerAddress.Should().Be("Buyer Address");
        result.BuyerPhone.Should().Be("+919876543210");
        result.GreetingLine.Should().Be("Dear John Buyer,");
        result.ItemDescription.Should().Be("Widget");
        result.CashChecked.Should().BeTrue();
        result.UpiChecked.Should().BeFalse();
        result.FinanceChecked.Should().BeFalse();
        result.FinanceCompanyDisplay.Should().Be("-");
        result.TotalAmountFormatted.Should().Be("Rs. 1,20,000");
        result.AmountInWords.Should().StartWith("In Words:").And.Contain("Rupees Only");
    }

    [Fact]
    public void ToPdfViewModel_NoCustomerName_UsesDearSirMadam()
    {
        var bill = new ManualBill
        {
            BillNumber = 1,
            CustomerName = "",
            Phone = "+919999999999",
            ItemDescription = "Item",
            AmountTotal = 100m,
            PaymentMode = PaymentMode.UPI,
            UpiAmount = 100m,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = ManualInvoiceTemplateMapper.ToPdfViewModel(bill, Settings());

        result.GreetingLine.Should().Be("Dear Sir / Madam,");
    }

    [Fact]
    public void ToPdfViewModel_FinanceMode_ShowsFinanceCompany()
    {
        var bill = new ManualBill
        {
            BillNumber = 1,
            CustomerName = "C",
            Phone = "+919999999999",
            ItemDescription = "Item",
            AmountTotal = 100m,
            PaymentMode = PaymentMode.Finance,
            FinanceAmount = 100m,
            FinanceCompany = "HDFC Bank",
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = ManualInvoiceTemplateMapper.ToPdfViewModel(bill, Settings());

        result.FinanceChecked.Should().BeTrue();
        result.FinanceCompanyDisplay.Should().Be("HDFC Bank");
        result.CashChecked.Should().BeFalse();
        result.UpiChecked.Should().BeFalse();
    }

    [Fact]
    public void ToPdfViewModel_FinanceMode_NoCompany_ShowsDash()
    {
        var bill = new ManualBill
        {
            BillNumber = 1,
            CustomerName = "C",
            Phone = "+919999999999",
            ItemDescription = "Item",
            AmountTotal = 100m,
            PaymentMode = PaymentMode.Finance,
            FinanceAmount = 100m,
            FinanceCompany = null,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = ManualInvoiceTemplateMapper.ToPdfViewModel(bill, Settings());

        result.FinanceCompanyDisplay.Should().Be("-");
    }

    [Fact]
    public void ToPdfViewModel_NullOptionalFields_ShowsDash()
    {
        var bill = new ManualBill
        {
            BillNumber = 1,
            CustomerName = "X",
            Phone = "+919999999999",
            Address = null,
            ItemDescription = "Desc",
            ChassisNumber = null,
            EngineNumber = null,
            Color = null,
            Notes = null,
            AmountTotal = 50m,
            PaymentMode = PaymentMode.Cash,
            CashAmount = 50m,
            CreatedAtUtc = DateTime.UtcNow
        };

        var result = ManualInvoiceTemplateMapper.ToPdfViewModel(bill, Settings());

        result.BuyerAddress.Should().Be("-");
        result.ChassisNo.Should().Be("-");
        result.EngineNo.Should().Be("-");
        result.Color.Should().Be("-");
        result.Notes.Should().Be("-");
    }

    [Fact]
    public void ToPdfViewModel_NullBill_Throws()
    {
        var act = () => ManualInvoiceTemplateMapper.ToPdfViewModel(null!, Settings());
        act.Should().Throw<ArgumentNullException>().WithParameterName("bill");
    }

    [Fact]
    public void ToPdfViewModel_NullSettings_Throws()
    {
        var bill = new ManualBill
        {
            BillNumber = 1,
            CustomerName = "X",
            Phone = "+919999999999",
            ItemDescription = "X",
            AmountTotal = 0m,
            PaymentMode = PaymentMode.Cash,
            CreatedAtUtc = DateTime.UtcNow
        };
        var act = () => ManualInvoiceTemplateMapper.ToPdfViewModel(bill, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("settings");
    }

    [Fact]
    public void ToPdfViewModel_WhenSellerNameProvided_UsesProvidedValue()
    {
        var bill = new ManualBill
        {
            BillNumber = 1,
            CustomerName = "Buyer",
            Phone = "+919999999999",
            ItemDescription = "Item",
            AmountTotal = 100m,
            PaymentMode = PaymentMode.Cash,
            CashAmount = 100m,
            SellerName = "ABC Motors",
            CreatedAtUtc = DateTime.UtcNow
        };
        var settings = Settings();

        var result = ManualInvoiceTemplateMapper.ToPdfViewModel(bill, settings);

        result.SellerName.Should().Be("ABC Motors");
        result.SellerAddress.Should().Be("123 Main St, City");
    }

    [Fact]
    public void ToPdfViewModel_WhenSellerNameNull_UsesDefaultShopName()
    {
        var bill = new ManualBill
        {
            BillNumber = 1,
            CustomerName = "Buyer",
            Phone = "+919999999999",
            ItemDescription = "Item",
            AmountTotal = 100m,
            PaymentMode = PaymentMode.Cash,
            SellerName = null,
            CreatedAtUtc = DateTime.UtcNow
        };
        var settings = Settings();

        var result = ManualInvoiceTemplateMapper.ToPdfViewModel(bill, settings);

        result.SellerName.Should().Be("SREE RAMALINGAM SONS");
    }

    [Fact]
    public void ToPdfViewModel_WhenSellerNameEmpty_UsesDefaultShopName()
    {
        var bill = new ManualBill
        {
            BillNumber = 1,
            CustomerName = "Buyer",
            Phone = "+919999999999",
            ItemDescription = "Item",
            AmountTotal = 100m,
            PaymentMode = PaymentMode.Cash,
            SellerName = "   ",
            CreatedAtUtc = DateTime.UtcNow
        };
        var settings = Settings();

        var result = ManualInvoiceTemplateMapper.ToPdfViewModel(bill, settings);

        result.SellerName.Should().Be("SREE RAMALINGAM SONS");
    }

    [Fact]
    public void ToPdfViewModel_WhenSellerNameProvided_TrimsValue()
    {
        var bill = new ManualBill
        {
            BillNumber = 1,
            CustomerName = "Buyer",
            Phone = "+919999999999",
            ItemDescription = "Item",
            AmountTotal = 100m,
            PaymentMode = PaymentMode.Cash,
            SellerName = "  XYZ Dealer  ",
            CreatedAtUtc = DateTime.UtcNow
        };
        var settings = Settings();

        var result = ManualInvoiceTemplateMapper.ToPdfViewModel(bill, settings);

        result.SellerName.Should().Be("XYZ Dealer");
    }
}
