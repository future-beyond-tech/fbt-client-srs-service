using SRS.Application.Constants;
using SRS.Application.DTOs;

namespace SRS.Application.Mapping;

/// <summary>
/// Maps Manual Bill PDF view model and delivery note settings to the shared Delivery Note template view model.
/// No business logic in template; all wording and structure built here.
/// </summary>
public static class ManualBillPdfMapper
{
    public static DeliveryNoteTemplateViewModel ToTemplateViewModel(ManualBillPdfViewModel viewModel, DeliveryNoteSettingsDto settings)
    {
        if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        var shopName = Safe(settings.ShopName, "SREE RAMALINGAM SONS");
        var shopAddress = SafeAddress(settings.ShopAddress);

        var bodyParagraph = "I have this day purchased the above item/service from you for the sum of " +
            viewModel.TotalAmountFormatted + " (" + viewModel.AmountInWords + ") and I have taken the delivery to my entire satisfaction.";
        var riskParagraph = "The entire risk is being borne by me / us from this time " + viewModel.SaleDate.ToString("dd-MM-yyyy hh:mm tt") + ".";

        var refText = string.IsNullOrWhiteSpace(viewModel.ItemDescription) ? "—" : viewModel.ItemDescription.Trim();

        var leftRows = new List<DetailRow>
        {
            new("Make / Model", SafeValue(viewModel.ItemDescription)),
            new("Chassis No", SafeValue(viewModel.ChassisNo)),
            new("Engine No", SafeValue(viewModel.EngineNo)),
            new("Color", SafeValue(viewModel.Color)),
            new("Notes", SafeValue(viewModel.Notes))
        };

        return new DeliveryNoteTemplateViewModel
        {
            ShopName = shopName,
            ShopTagline = settings.ShopTagline,
            ShopTagline2 = settings.ShopTagline2,
            ShopAddress = shopAddress,
            CenterShopNameInHeader = true,

            BillNumber = viewModel.BillNumber,
            BillDate = viewModel.SaleDate.ToString("dd-MM-yyyy"),

            TitleLine1 = "DELIVERY NOTE",
            TitleLine2 = "Only on Commission Basis",
            TitleLine3 = shopName,

            SellerLabel = "SELLER",
            SellerName = Safe(viewModel.SellerName, shopName),
            SellerAddress = SafeAddress(viewModel.SellerAddress),

            BuyerLabel = "BUYER",
            BuyerName = Safe(viewModel.BuyerName, "—"),
            BuyerAddress = SafeAddress(viewModel.BuyerAddress),
            BuyerPhone = Safe(viewModel.BuyerPhone, "—"),

            GreetingLine = Safe(viewModel.GreetingLine, "Sir / Madam,"),

            RefText = refText,
            BodyParagraph = bodyParagraph,
            RiskParagraph = riskParagraph,

            DetailsLeftTitle = "VEHICLE DETAILS",
            DetailsLeftRows = leftRows,

            DetailsRightTitle = "PAYMENT DETAILS",
            DetailsRightRows = Array.Empty<DetailRow>(),
            UsePaymentCheckboxes = true,
            PaymentCashChecked = viewModel.CashChecked,
            PaymentUpiChecked = viewModel.UpiChecked,
            PaymentFinanceChecked = viewModel.FinanceChecked,
            FinanceName = viewModel.FinanceChecked ? SafeValue(viewModel.FinanceCompanyDisplay) : "-",

            TamilTerms = NormalizeTamilTerms(settings.TamilTermsAndConditions, settings.TermsAndConditions),
            FooterThankYou = Safe(settings.FooterText, "Thank you for your purchase."),
            SignatureLineLabel = Safe(settings.SignatureLine, "Authorized Signature")
        };
    }

    private static string Safe(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string SafeValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();

    private static string SafeAddress(string? value) =>
        string.IsNullOrWhiteSpace(value) || value.Contains("not configured", StringComparison.OrdinalIgnoreCase)
            ? "—"
            : value.Trim();

    /// <summary>Preferred: TamilTermsAndConditions from config/DB; else TermsAndConditions; else default constants.</summary>
    private static string NormalizeTamilTerms(string? tamilTerms, string? termsAndConditions)
    {
        if (!string.IsNullOrWhiteSpace(tamilTerms)) return tamilTerms.Trim();
        if (!string.IsNullOrWhiteSpace(termsAndConditions)) return termsAndConditions.Trim();
        return PdfContentConstants.DefaultTamilTerms;
    }
}
