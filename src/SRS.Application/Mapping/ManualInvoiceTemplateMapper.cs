using SRS.Application.Common;
using SRS.Application.DTOs;
using SRS.Domain.Entities;
using SRS.Domain.Enums;

namespace SRS.Application.Mapping;

/// <summary>
/// Maps ManualBill and delivery note settings to the view model used by the Manual Billing PDF template only.
/// Does not mix with Sales invoice logic.
/// PDF behaviour: when seller address is provided it is used; otherwise shop address. Both buyer and seller names include title (Mr/Miss/Mrs) when provided.
/// </summary>
public static class ManualInvoiceTemplateMapper
{
    /// <summary>
    /// Maps entity and settings to the PDF view model. Null-safe; optional fields show "-" when null/empty.
    /// </summary>
    public static ManualBillPdfViewModel ToPdfViewModel(ManualBill bill, DeliveryNoteSettingsDto settings)
    {
        if (bill == null)
            throw new ArgumentNullException(nameof(bill));
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        var cashChecked = bill.PaymentMode == PaymentMode.Cash;
        var upiChecked = bill.PaymentMode == PaymentMode.UPI;
        var financeChecked = bill.PaymentMode == PaymentMode.Finance;

        var sellerNameRaw = string.IsNullOrWhiteSpace(bill.SellerName)
            ? (settings.ShopName ?? "—")
            : bill.SellerName.Trim();
        // PDF: include seller title (Mr/Miss/Mrs) when provided
        var sellerName = string.IsNullOrWhiteSpace(bill.SellerNameTitle)
            ? sellerNameRaw
            : $"{bill.SellerNameTitle.Trim()} {sellerNameRaw}".Trim();

        // PDF: when seller address is provided use it; otherwise use shop address
        var sellerAddress = !string.IsNullOrWhiteSpace(bill.SellerAddress)
            ? bill.SellerAddress.Trim()
            : (string.IsNullOrWhiteSpace(settings.ShopAddress) ? "—" : settings.ShopAddress.Trim());

        var buyerNameRaw = string.IsNullOrWhiteSpace(bill.CustomerName) ? "-" : bill.CustomerName.Trim();
        // PDF: include buyer title (Mr/Miss/Mrs) when provided
        var buyerName = string.IsNullOrWhiteSpace(bill.CustomerNameTitle)
            ? buyerNameRaw
            : $"{bill.CustomerNameTitle.Trim()} {buyerNameRaw}".Trim();

        // Greeting uses title + name for "Dear Mr John," or "Dear Sir / Madam," when no name
        var greetingName = string.IsNullOrWhiteSpace(bill.CustomerName)
            ? "Sir / Madam"
            : (string.IsNullOrWhiteSpace(bill.CustomerNameTitle)
                ? bill.CustomerName.Trim()
                : $"{bill.CustomerNameTitle.Trim()} {bill.CustomerName.Trim()}".Trim());

        return new ManualBillPdfViewModel
        {
            BillNumber = bill.BillNumber,
            SaleDate = bill.CreatedAtUtc,

            SellerName = sellerName,
            SellerAddress = sellerAddress,

            BuyerName = buyerName,
            BuyerAddress = string.IsNullOrWhiteSpace(bill.Address) ? "-" : bill.Address,
            BuyerPhone = bill.Phone ?? "-",

            GreetingLine = $"Dear {greetingName},",

            ItemDescription = string.IsNullOrWhiteSpace(bill.ItemDescription) ? "-" : bill.ItemDescription,
            ChassisNo = string.IsNullOrWhiteSpace(bill.ChassisNumber) ? "-" : bill.ChassisNumber,
            EngineNo = string.IsNullOrWhiteSpace(bill.EngineNumber) ? "-" : bill.EngineNumber,
            Color = string.IsNullOrWhiteSpace(bill.Color) ? "-" : bill.Color,
            Notes = string.IsNullOrWhiteSpace(bill.Notes) ? "-" : bill.Notes,

            CashChecked = cashChecked,
            UpiChecked = upiChecked,
            FinanceChecked = financeChecked,
            FinanceCompanyDisplay = financeChecked && !string.IsNullOrWhiteSpace(bill.FinanceCompany)
                ? bill.FinanceCompany!.Trim()
                : "-",

            TotalAmountFormatted = "Rs. " + NumberToWordsConverter.FormatIndianCurrency(bill.AmountTotal),
            AmountInWords = "In Words: " + NumberToWordsConverter.ToRupeesInWords(bill.AmountTotal)
        };
    }
}
