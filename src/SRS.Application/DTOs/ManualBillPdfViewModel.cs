namespace SRS.Application.DTOs;

/// <summary>
/// View model for Manual Billing PDF template only. Not used for Sales invoice.
/// </summary>
public class ManualBillPdfViewModel
{
    public int BillNumber { get; set; }
    public DateTime SaleDate { get; set; }

    public string SellerName { get; set; } = null!;
    public string SellerAddress { get; set; } = null!;

    public string BuyerName { get; set; } = null!;
    public string BuyerAddress { get; set; } = "-";
    public string BuyerPhone { get; set; } = null!;

    public string GreetingLine { get; set; } = "Dear Sir / Madam,";

    public string ItemDescription { get; set; } = "-";
    public string ChassisNo { get; set; } = "-";
    public string EngineNo { get; set; } = "-";
    public string Color { get; set; } = "-";
    public string Notes { get; set; } = "-";

    public bool CashChecked { get; set; }
    public bool UpiChecked { get; set; }
    public bool FinanceChecked { get; set; }
    public string FinanceCompanyDisplay { get; set; } = "-";

    public string TotalAmountFormatted { get; set; } = null!;
    public string AmountInWords { get; set; } = null!;
}
