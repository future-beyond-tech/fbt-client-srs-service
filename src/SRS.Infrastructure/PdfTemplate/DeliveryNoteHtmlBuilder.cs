using System.Text;
using SRS.Application.Constants;
using SRS.Application.DTOs;

namespace SRS.Infrastructure.PdfTemplate;

/// <summary>
/// Builds the full HTML document for the Delivery Note PDF (Manual Billing).
/// Uses inline CSS so wkhtmltopdf does not require local file access for styles.
/// Photo is embedded as data URL when provided.
/// </summary>
public static class DeliveryNoteHtmlBuilder
{
    private const string CheckboxChecked = "☑";
    private const string CheckboxUnchecked = "☐";

    public static string Build(DeliveryNoteTemplateViewModel vm, string? photoDataUrl, string? notoSansTamilBase64 = null)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">");
        sb.Append("<style>");
        AppendStyles(sb, notoSansTamilBase64);
        sb.Append("</style></head><body>");

        // Header bar (centered: shop name, taglines, address – no flex)
        sb.Append("<header class=\"header-bar\">");
        sb.Append("<div class=\"header-inner\">");
        sb.Append($"<div class=\"shop-name\">{Escape(vm.ShopName)}</div>");
        if (!string.IsNullOrWhiteSpace(vm.ShopTagline))
            sb.Append($"<div class=\"shop-tagline\">{Escape(vm.ShopTagline)}</div>");
        if (!string.IsNullOrWhiteSpace(vm.ShopTagline2))
            sb.Append($"<div class=\"shop-tagline2\">{Escape(vm.ShopTagline2)}</div>");
        sb.Append($"<div class=\"header-address\">{Escape(vm.ShopAddress)}</div>");
        sb.Append("</div></header>");

        // Bill meta
        sb.Append("<div class=\"bill-meta\">");
        sb.Append($"<span>Bill No: {Escape(vm.BillNumber.ToString())}</span>");
        sb.Append($"<span class=\"bill-date\">Date: {Escape(vm.BillDate)}</span>");
        sb.Append("</div>");

        // Title block + photo
        sb.Append("<div class=\"title-row\">");
        sb.Append("<div class=\"title-block\">");
        sb.Append($"<h1 class=\"title-line1\">{Escape(vm.TitleLine1)}</h1>");
        sb.Append($"<div class=\"title-line2\">{Escape(vm.TitleLine2)}</div>");
        sb.Append($"<div class=\"title-line3\">{Escape(vm.TitleLine3)}</div>");
        sb.Append("<hr class=\"title-divider\"/>");
        sb.Append("</div>");
        sb.Append("<div class=\"photo-frame\">");
        if (!string.IsNullOrWhiteSpace(photoDataUrl))
            sb.Append($"<img src=\"{photoDataUrl}\" alt=\"Photo\" class=\"photo-img\"/>");
        else
            sb.Append("<span class=\"photo-placeholder\">Photo unavailable</span>");
        sb.Append("</div>");
        sb.Append("</div>");

        // Cards
        sb.Append("<div class=\"cards-row\">");
        sb.Append($"<div class=\"card\"><div class=\"card-header\">{Escape(vm.SellerLabel)}:</div><div class=\"card-body\"><strong>{Escape(vm.SellerName)}</strong><br/>{Escape(vm.SellerAddress)}</div></div>");
        sb.Append("<div class=\"card-spacer\"></div>");
        sb.Append($"<div class=\"card\"><div class=\"card-header\">{Escape(vm.BuyerLabel)}:</div><div class=\"card-body\"><strong>{Escape(vm.BuyerName)}</strong><br/>{Escape(vm.BuyerAddress)}");
        if (!string.IsNullOrWhiteSpace(vm.BuyerPhone))
            sb.Append($"<br/>Phone: {Escape(vm.BuyerPhone)}");
        sb.Append("</div></div>");
        sb.Append("</div>");

        // Greeting + Ref strip
        sb.Append($"<p class=\"greeting\">{Escape(vm.GreetingLine)}</p>");
        sb.Append($"<div class=\"ref-strip\">Ref: {Escape(vm.RefText)}</div>");

        // Body
        sb.Append($"<p class=\"body-p\">{Escape(vm.BodyParagraph)}</p>");
        if (!string.IsNullOrEmpty(vm.RiskParagraph))
            sb.Append($"<p class=\"risk-p\">{Escape(vm.RiskParagraph)}</p>");

        // Two-column tables
        sb.Append("<div class=\"tables-row\">");
        sb.Append("<div class=\"table-wrap\">");
        sb.Append($"<div class=\"table-header\">{Escape(vm.DetailsLeftTitle)}</div>");
        sb.Append("<table class=\"details-table\"><tbody>");
        foreach (var row in vm.DetailsLeftRows ?? Array.Empty<DetailRow>())
        {
            sb.Append("<tr><td class=\"td-label\">").Append(Escape(row.Label)).Append("</td><td class=\"td-value\">").Append(Escape(row.Value ?? "-")).Append("</td></tr>");
        }
        sb.Append("</tbody></table></div>");
        sb.Append("<div class=\"table-spacer\"></div>");
        sb.Append("<div class=\"table-wrap\">");
        sb.Append($"<div class=\"table-header\">{Escape(vm.DetailsRightTitle)}</div>");
        if (vm.UsePaymentCheckboxes)
        {
            sb.Append("<div class=\"payment-checkboxes\">");
            sb.Append("<div class=\"payment-checkbox-row\"><span class=\"payment-checkbox-symbol\">").Append(vm.PaymentCashChecked ? CheckboxChecked : CheckboxUnchecked).Append("</span><span class=\"payment-checkbox-label\">Cash</span></div>");
            sb.Append("<div class=\"payment-checkbox-row\"><span class=\"payment-checkbox-symbol\">").Append(vm.PaymentUpiChecked ? CheckboxChecked : CheckboxUnchecked).Append("</span><span class=\"payment-checkbox-label\">UPI</span></div>");
            sb.Append("<div class=\"payment-checkbox-row\"><span class=\"payment-checkbox-symbol\">").Append(vm.PaymentFinanceChecked ? CheckboxChecked : CheckboxUnchecked).Append("</span><span class=\"payment-checkbox-label\">Finance</span></div>");
            sb.Append($"<div class=\"finance-name\">Finance Name: {Escape(vm.FinanceName ?? "-")}</div>");
            sb.Append("</div>");
        }
        else
        {
            sb.Append("<table class=\"details-table\"><tbody>");
            foreach (var row in vm.DetailsRightRows ?? Array.Empty<DetailRow>())
            {
                sb.Append("<tr><td class=\"td-label\">").Append(Escape(row.Label)).Append("</td><td class=\"td-value\">").Append(Escape(row.Value ?? "-")).Append("</td></tr>");
            }
            sb.Append("</tbody></table>");
        }
        sb.Append("</div></div>");

        // Mandatory Tamil terms (always; from single source of truth)
        sb.Append("<div class=\"tamil-terms\">");
        foreach (var line in PdfContentConstants.TamilTerms)
        {
            if (string.IsNullOrEmpty(line)) continue;
            sb.Append("<div class=\"tamil-term\">").Append(Escape(line)).Append("</div>");
        }
        sb.Append("</div>");

        // Footer
        sb.Append($"<p class=\"footer-thanks\">{Escape(vm.FooterThankYou)}</p>");
        sb.Append("<div class=\"signatures\">");
        sb.Append($"<div class=\"sig-authorized\">{Escape(vm.SignatureLineLabel)} _____________________</div>");
        sb.Append("<div class=\"sig-row\">");
        sb.Append("<span>Signature of Dealer _____________________</span>");
        sb.Append("<span>Signature of Buyer _____________________</span>");
        sb.Append("</div></div>");

        sb.Append("</body></html>");
        return sb.ToString();
    }

    private static void AppendStyles(StringBuilder sb, string? notoSansTamilBase64 = null)
    {
        if (!string.IsNullOrWhiteSpace(notoSansTamilBase64))
        {
            sb.Append("@font-face{font-family:'Noto Sans Tamil';src:url(data:font/ttf;base64,");
            sb.Append(notoSansTamilBase64);
            sb.Append(") format('truetype');}");
        }
        sb.Append("body{font-family:'Noto Sans Tamil',Arial,sans-serif;font-size:12pt;line-height:1.45;color:#222;margin:0;padding:32px;}");
        sb.Append(".header-bar{background:#1565c0;color:#fff;padding:14px 12px;text-align:center;}");
        sb.Append(".header-inner{max-width:100%;}");
        sb.Append(".shop-name{font-weight:bold;font-size:18pt;}");
        sb.Append(".shop-tagline{font-size:11pt;}");
        sb.Append(".shop-tagline2{font-size:10pt;}");
        sb.Append(".header-address{font-size:11pt;margin-top:6px;line-height:1.35;}");
        sb.Append(".bill-meta{margin-bottom:14px;display:flex;justify-content:space-between;align-items:center;font-size:11pt;}");
        sb.Append(".bill-date{float:right;}");
        sb.Append(".title-row{display:flex;justify-content:space-between;align-items:flex-start;margin:16px 0;}");
        sb.Append(".title-block{flex:1;text-align:center;}");
        sb.Append(".title-line1{font-size:21pt;font-weight:bold;margin:0 0 4px;}");
        sb.Append(".title-line2{font-size:12.5pt;}");
        sb.Append(".title-line3{font-size:12pt;}");
        sb.Append(".title-divider{border:0;border-top:1px solid #bdbdbd;margin:6px 0 0;}");
        sb.Append(".photo-frame{width:115px;height:115px;border:1px solid #d0d0d0;border-radius:8px;padding:3px;display:flex;align-items:center;justify-content:center;overflow:hidden;flex-shrink:0;}");
        sb.Append(".photo-img{width:100%;height:100%;object-fit:cover;}");
        sb.Append(".photo-placeholder{font-size:10pt;color:#666;}");
        sb.Append(".cards-row{display:flex;gap:14px;margin:16px 0;}");
        sb.Append(".card{flex:1;background:#fafafa;border:1px solid #e8e8e8;border-radius:8px;}");
        sb.Append(".card-header{background:#d4a84b;padding:6px 8px;font-weight:bold;font-size:11pt;border-radius:8px 8px 0 0;}");
        sb.Append(".card-body{padding:12px;}");
        sb.Append(".card-spacer{width:14px;}");
        sb.Append(".greeting{margin:0 0 8px;font-size:12pt;}");
        sb.Append(".ref-strip{background:#d4a84b;padding:8px 10px;font-weight:bold;margin-bottom:14px;}");
        sb.Append(".body-p,.risk-p{margin:0 0 6px;line-height:1.45;}");
        sb.Append(".risk-p{margin-top:6px;}");
        sb.Append(".tables-row{display:flex;gap:14px;margin:16px 0;}");
        sb.Append(".table-wrap{flex:1;}");
        sb.Append(".table-header{background:#90caf9;padding:8px 10px;font-weight:bold;font-size:10.5pt;}");
        sb.Append(".details-table{width:100%;border-collapse:collapse;}");
        sb.Append(".details-table td{border-bottom:1px solid #e8e8e8;padding:6px 8px;font-size:10.5pt;}");
        sb.Append(".td-label{font-weight:600;}");
        sb.Append(".table-spacer{width:14px;}");
        sb.Append(".payment-checkboxes{font-size:10.5pt;padding:6px;}");
        sb.Append(".payment-checkbox-row{margin:5px 0;}");
        sb.Append(".payment-checkbox-symbol{margin-right:6px;}");
        sb.Append(".finance-name{margin-top:10px;}");
        sb.Append(".tamil-terms{margin-top:20px;padding:12px 0;color:#c00;font-size:10.5pt;line-height:1.45;}");
        sb.Append(".tamil-term{margin:8px 0;}");
        sb.Append(".footer-thanks{margin:20px 0 10px;font-size:12pt;}");
        sb.Append(".signatures{margin-top:24px;}");
        sb.Append(".sig-authorized{text-align:right;margin-bottom:10px;font-size:10.5pt;}");
        sb.Append(".sig-row{display:flex;justify-content:space-between;font-size:10.5pt;}");
    }

    private static string Escape(string? s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }
}
