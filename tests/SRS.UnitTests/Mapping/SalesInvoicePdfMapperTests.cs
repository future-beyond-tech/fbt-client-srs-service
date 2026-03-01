using FluentAssertions;
using SRS.Application.DTOs;
using SRS.Application.Mapping;
using SRS.Domain.Enums;
using Xunit;

namespace SRS.UnitTests.Mapping;

public sealed class SalesInvoicePdfMapperTests
{
    private static DeliveryNoteSettingsDto Settings() => new()
    {
        ShopName = "Test Shop",
        ShopAddress = "Test Address",
        FooterText = "Thank you.",
        SignatureLine = "Authorized",
    };

    [Fact]
    public void ToTemplateViewModel_EmptyOrNullVehicleFields_MapsToDash()
    {
        var invoice = new SaleInvoiceDto
        {
            BillNumber = 1,
            SaleDate = new DateTime(2026, 2, 28),
            CustomerName = "Customer",
            Phone = "123",
            Address = "Addr",
            PhotoUrl = "",
            CustomerPhone = "123",
            CustomerAddress = "Addr",
            CustomerPhotoUrl = "",
            VehicleBrand = "",
            VehicleModel = "",
            RegistrationNumber = "",
            ChassisNumber = null,
            EngineNumber = null,
            Colour = null,
            SellingPrice = 100m,
            PaymentMode = PaymentMode.Cash,
            CashAmount = 100m,
            UpiAmount = null,
            FinanceAmount = null,
            FinanceCompany = null,
        };
        var settings = Settings();

        var result = SalesInvoicePdfMapper.ToTemplateViewModel(invoice, settings);

        result.DetailsLeftRows.Should().NotBeEmpty();
        result.DetailsLeftRows.Should().Contain(r => r.Label == "Chassis No" && r.Value == "—");
        result.DetailsLeftRows.Should().Contain(r => r.Label == "Engine No" && r.Value == "—");
        result.DetailsLeftRows.Should().Contain(r => r.Label == "Color" && r.Value == "—");
    }

    [Fact]
    public void ToTemplateViewModel_SellerBuyerLabels_AreSellerAndBuyer()
    {
        var invoice = new SaleInvoiceDto
        {
            BillNumber = 1,
            SaleDate = DateTime.UtcNow,
            CustomerName = "Buyer",
            Phone = "1",
            Address = "A",
            PhotoUrl = "",
            CustomerPhone = "1",
            CustomerAddress = "A",
            CustomerPhotoUrl = "",
            VehicleBrand = "B",
            VehicleModel = "M",
            RegistrationNumber = "R",
            SellingPrice = 1m,
            PaymentMode = PaymentMode.Cash,
            CashAmount = 1m,
        };
        var result = SalesInvoicePdfMapper.ToTemplateViewModel(invoice, Settings());

        result.SellerLabel.Should().Be("SELLER");
        result.BuyerLabel.Should().Be("BUYER");
        result.GreetingLine.Should().Be("Sir / Madam,");
    }

    [Fact]
    public void ToTemplateViewModel_UsePaymentCheckboxes_True()
    {
        var invoice = new SaleInvoiceDto
        {
            BillNumber = 1,
            SaleDate = DateTime.UtcNow,
            CustomerName = "C",
            Phone = "1",
            Address = "A",
            PhotoUrl = "",
            CustomerPhone = "1",
            CustomerAddress = "A",
            CustomerPhotoUrl = "",
            VehicleBrand = "B",
            VehicleModel = "M",
            RegistrationNumber = "R",
            SellingPrice = 100m,
            PaymentMode = PaymentMode.Finance,
            FinanceAmount = 50m,
            FinanceCompany = "HDFC",
        };
        var result = SalesInvoicePdfMapper.ToTemplateViewModel(invoice, Settings());

        result.UsePaymentCheckboxes.Should().BeTrue();
        result.PaymentFinanceChecked.Should().BeTrue();
        result.FinanceName.Should().Be("HDFC");
    }
}
