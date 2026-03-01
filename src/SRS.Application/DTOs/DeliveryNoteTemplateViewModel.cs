namespace SRS.Application.DTOs;

/// <summary>
/// View model for the shared branded Delivery Note PDF template. Used by both Sales and Manual billing.
/// All display strings are pre-mapped; template has no business logic.
/// </summary>
public class DeliveryNoteTemplateViewModel
{
    public string ShopName { get; set; } = "SHREE RAMALINGAM SONS";
    public string? ShopTagline { get; set; }
    public string? ShopTagline2 { get; set; }
    public string ShopAddress { get; set; } = "H.O.: 154, Pycrofts Road, Royapettah (Opp. Sub Reg. Office) Chennai - 600 014";

    /// <summary>When true, shop name (and taglines) are centered in the blue header bar.</summary>
    public bool CenterShopNameInHeader { get; set; }

    public int BillNumber { get; set; }
    public string BillDate { get; set; } = null!;

    public string TitleLine1 { get; set; } = "DELIVERY NOTE";
    public string TitleLine2 { get; set; } = "Only on Commission Basis";
    public string TitleLine3 { get; set; } = "SREE RAMALINGAM SONS";

    /// <summary>Card header label, e.g. "FROM" (Sales) or "SELLER" (Manual).</summary>
    public string SellerLabel { get; set; } = "FROM";
    public string SellerName { get; set; } = null!;
    public string SellerAddress { get; set; } = "—";

    /// <summary>Card header label, e.g. "TO" (Sales) or "BUYER" (Manual).</summary>
    public string BuyerLabel { get; set; } = "TO";
    public string BuyerName { get; set; } = null!;
    public string BuyerAddress { get; set; } = "—";
    public string BuyerPhone { get; set; } = "—";

    public string GreetingLine { get; set; } = "Sir,";

    public string RefText { get; set; } = "—";

    public string BodyParagraph { get; set; } = null!;
    public string RiskParagraph { get; set; } = "";

    public string DetailsLeftTitle { get; set; } = "VEHICLE DETAILS";
    public IReadOnlyList<DetailRow> DetailsLeftRows { get; set; } = Array.Empty<DetailRow>();

    public string DetailsRightTitle { get; set; } = "PAYMENT DETAILS";
    public IReadOnlyList<DetailRow> DetailsRightRows { get; set; } = Array.Empty<DetailRow>();

    /// <summary>When true, right column renders checkbox-style payment (Cash/UPI/Finance) + Finance Name.</summary>
    public bool UsePaymentCheckboxes { get; set; }
    public bool PaymentCashChecked { get; set; }
    public bool PaymentUpiChecked { get; set; }
    public bool PaymentFinanceChecked { get; set; }
    /// <summary>Display value for "Finance Name: ..." when Finance is selected; otherwise show "Finance Name: -".</summary>
    public string FinanceName { get; set; } = "—";

    public string TamilTerms { get; set; } = "";

    public string FooterThankYou { get; set; } = "Thank you for your purchase.";
    public string SignatureLineLabel { get; set; } = "Authorized Signature";
}

public record DetailRow(string Label, string Value);
