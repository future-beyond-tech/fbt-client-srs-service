using SRS.Application.Common;
using SRS.Application.Constants;
using SRS.Application.DTOs;
using SRS.Domain.Enums;

namespace SRS.Application.Mapping;

/// <summary>
/// Maps Sales invoice DTO and delivery note settings to the shared Delivery Note template view model.
/// Same template family as Manual Billing: SELLER/BUYER cards, payment checkboxes, Tamil terms. No business logic in template.
/// </summary>
public static class SalesInvoicePdfMapper
{
    public static DeliveryNoteTemplateViewModel ToTemplateViewModel(SaleInvoiceDto invoice, DeliveryNoteSettingsDto settings)
    {
        if (invoice == null) throw new ArgumentNullException(nameof(invoice));
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        var shopName = Safe(settings.ShopName, "SREE RAMALINGAM SONS");
        var shopAddress = SafeAddress(settings.ShopAddress);
        var amountFormatted = "Rs. " + NumberToWordsConverter.FormatIndianCurrency(invoice.SellingPrice);
        var amountWords = NumberToWordsConverter.ToRupeesInWords(invoice.SellingPrice);
        var deliveryTimeStr = invoice.DeliveryTime.HasValue
            ? DateTime.Today.Add(invoice.DeliveryTime.Value).ToString("dd-MM-yyyy hh:mm tt")
            : DateTime.Now.ToString("dd-MM-yyyy hh:mm tt");

        var bodyParagraph = "I have this day purchased the above vehicle from you for the sum of " +
            amountFormatted + " (Rupees in Words: " + amountWords + ") and I have taken the delivery of the said vehicle to my entire satisfaction.";
        var riskParagraph = "The entire risk is being borne by me / us from this time " + deliveryTimeStr + ".";

        var refText = string.Join(" / ",
            new[] { invoice.RegistrationNumber, invoice.VehicleBrand, invoice.VehicleModel }
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!.Trim()));
        if (string.IsNullOrEmpty(refText)) refText = "—";

        var leftRows = new List<DetailRow>
        {
            new("Make / Model", SafeValue((invoice.VehicleBrand + " " + invoice.VehicleModel).Trim())),
            new("Chassis No", SafeValue(invoice.ChassisNumber)),
            new("Engine No", SafeValue(invoice.EngineNumber)),
            new("Color", SafeValue(invoice.Colour)),
            new("Registration", SafeValue(invoice.RegistrationNumber))
        };

        bool cashChecked = invoice.CashAmount.HasValue && invoice.CashAmount > 0;
        bool upiChecked = invoice.UpiAmount.HasValue && invoice.UpiAmount > 0;
        bool financeChecked = invoice.PaymentMode == PaymentMode.Finance || (invoice.FinanceAmount.HasValue && invoice.FinanceAmount > 0);
        string financeName = financeChecked ? SafeValue(invoice.FinanceCompany) : "—";

        return new DeliveryNoteTemplateViewModel
        {
            ShopName = shopName,
            ShopTagline = settings.ShopTagline,
            ShopTagline2 = settings.ShopTagline2,
            ShopAddress = shopAddress,
            CenterShopNameInHeader = true,

            BillNumber = invoice.BillNumber,
            BillDate = invoice.SaleDate.ToString("dd-MM-yyyy"),

            TitleLine1 = "DELIVERY NOTE",
            TitleLine2 = "Only on Commission Basis",
            TitleLine3 = shopName,

            SellerLabel = "SELLER",
            SellerName = shopName,
            SellerAddress = shopAddress,

            BuyerLabel = "BUYER",
            BuyerName = Safe(invoice.CustomerName, "—"),
            BuyerAddress = SafeAddress(invoice.Address),
            BuyerPhone = Safe(invoice.Phone, "—"),

            GreetingLine = "Sir / Madam,",

            RefText = refText,
            BodyParagraph = bodyParagraph,
            RiskParagraph = riskParagraph,

            DetailsLeftTitle = "VEHICLE DETAILS",
            DetailsLeftRows = leftRows,

            DetailsRightTitle = "PAYMENT DETAILS",
            DetailsRightRows = Array.Empty<DetailRow>(),
            UsePaymentCheckboxes = true,
            PaymentCashChecked = cashChecked,
            PaymentUpiChecked = upiChecked,
            PaymentFinanceChecked = financeChecked,
            FinanceName = financeName,

            TamilTerms = NormalizeTamilTerms(settings.TamilTermsAndConditions, settings.TermsAndConditions),
            FooterThankYou = Safe(settings.FooterText, "Thank you for your purchase."),
            SignatureLineLabel = Safe(settings.SignatureLine, "Authorized Signature")
        };
    }

    private static string Safe(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string SafeValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    private static string SafeAddress(string? value) =>
        string.IsNullOrWhiteSpace(value) || value.Contains("not configured", StringComparison.OrdinalIgnoreCase)
            ? "—"
            : value.Trim();

    private static string NormalizeTamilTerms(string? tamilTerms, string? termsAndConditions)
    {
        if (!string.IsNullOrWhiteSpace(tamilTerms)) return tamilTerms.Trim();
        if (!string.IsNullOrWhiteSpace(termsAndConditions)) return termsAndConditions.Trim();
        return PdfContentConstants.DefaultTamilTerms;
    }
}
