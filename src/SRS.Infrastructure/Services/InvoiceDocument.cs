using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SRS.Application.DTOs;
using SRS.Domain.Enums;

namespace SRS.Application.Services;

public class InvoiceDocument(
    SaleInvoiceDto invoice,
    byte[]? customerPhotoBytes,
    byte[]? shopLogoBytes,
    DeliveryNoteSettingsDto settings) : IDocument
{
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(24);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Header()
                .PaddingBottom(8)
                .Element(ComposeHeader);

            page.Content().Column(column =>
            {
                column.Spacing(10);

                column.Item().Element(ComposeBillInfo);
                column.Item().Element(ComposeCustomerSection);
                column.Item().Element(ComposeVehicleSection);
                column.Item().Element(ComposePaymentSection);
                column.Item().Element(ComposeDeliveryLegalDeclarationSection);
            });

            page.Footer()
                .PaddingTop(8)
                .AlignCenter()
                .Text(settings.FooterText ?? "Thank you for your purchase.")
                .Italic()
                .FontSize(9);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            if (shopLogoBytes is { Length: > 0 })
            {
                row.ConstantItem(72)
                    .Height(72)
                    .PaddingRight(8)
                    .Element(logo => logo.Image(shopLogoBytes).FitArea());
            }

            row.RelativeItem().AlignMiddle().Column(column =>
            {
                column.Spacing(2);
                column.Item().AlignCenter().Text(settings.ShopName).SemiBold().FontSize(20);
                column.Item().AlignCenter().Text(settings.ShopAddress).FontSize(10);

                if (!string.IsNullOrWhiteSpace(settings.GSTNumber))
                {
                    column.Item().AlignCenter().Text($"GST: {settings.GSTNumber}").FontSize(9);
                }

                if (!string.IsNullOrWhiteSpace(settings.ContactNumber))
                {
                    column.Item().AlignCenter().Text($"Contact: {settings.ContactNumber}").FontSize(9);
                }
            });
        });
    }

    private void ComposeBillInfo(IContainer container)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
        {
            column.Spacing(4);
            column.Item().Text("Bill Information").SemiBold().FontSize(12);
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Bill Number: {invoice.BillNumber}");
                row.RelativeItem().Text($"Sale Date: {invoice.SaleDate:dd MMM yyyy}");
                row.RelativeItem().Text($"Delivery Time: {FormatTime(invoice.DeliveryTime)}");
            });
        });
    }

    private void ComposeCustomerSection(IContainer container)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
        {
            column.Spacing(6);
            column.Item().Text("Customer Details").SemiBold().FontSize(12);
            column.Item().Row(row =>
            {
                row.RelativeItem(3).Column(details =>
                {
                    details.Spacing(3);
                    details.Item().Text($"Name: {invoice.CustomerName}");
                    details.Item().Text($"Father Name: {invoice.FatherName ?? "N/A"}");
                    details.Item().Text($"Phone: {invoice.Phone}");
                    details.Item().Text($"Address: {invoice.Address ?? "N/A"}");
                    details.Item().Text($"ID Proof: {invoice.IdProofNumber ?? "N/A"}");
                });

                row.ConstantItem(140).Height(160).Border(1).Padding(4).Element(photoContainer =>
                {
                    if (customerPhotoBytes is { Length: > 0 })
                    {
                        photoContainer.Image(customerPhotoBytes).FitArea();
                    }
                    else
                    {
                        photoContainer.AlignCenter().AlignMiddle().Text("Photo unavailable").FontSize(9);
                    }
                });
            });
        });
    }

    private void ComposeVehicleSection(IContainer container)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
        {
            column.Spacing(6);
            column.Item().Text("Vehicle Details").SemiBold().FontSize(12);
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                });

                AddTableCell(table, "Brand");
                AddTableCell(table, invoice.VehicleBrand, false);
                AddTableCell(table, "Model");
                AddTableCell(table, invoice.VehicleModel, false);

                AddTableCell(table, "Registration No.");
                AddTableCell(table, invoice.RegistrationNumber, false);
                AddTableCell(table, "Colour");
                AddTableCell(table, invoice.Colour ?? "N/A", false);

                AddTableCell(table, "Chassis No.");
                AddTableCell(table, invoice.ChassisNumber ?? "N/A", false);
                AddTableCell(table, "Engine No.");
                AddTableCell(table, invoice.EngineNumber ?? "N/A", false);

                AddTableCell(table, "Selling Price");
                AddTableCell(table, FormatAmount(invoice.SellingPrice), false);
                AddTableCell(table, string.Empty);
                AddTableCell(table, string.Empty, false);
            });
        });
    }

    private void ComposePaymentSection(IContainer container)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
        {
            column.Spacing(6);
            column.Item().Text("Payment Details").SemiBold().FontSize(12);
            column.Item().Text($"{GetCheckbox(invoice.PaymentMode == PaymentMode.Cash)} Cash");
            column.Item().Text($"{GetCheckbox(invoice.PaymentMode == PaymentMode.UPI)} UPI");
            column.Item().Text(
                $"{GetCheckbox(invoice.PaymentMode == PaymentMode.Finance)} Finance" +
                $"{(invoice.PaymentMode == PaymentMode.Finance && !string.IsNullOrWhiteSpace(invoice.FinanceCompany) ? $" - {invoice.FinanceCompany}" : string.Empty)}");
            column.Item().Text($"Cash: {FormatAmount(invoice.CashAmount ?? 0m)}");
            column.Item().Text($"UPI: {FormatAmount(invoice.UpiAmount ?? 0m)}");
            column.Item().Text($"Finance: {FormatAmount(invoice.FinanceAmount ?? 0m)}");
            column.Item().Text($"Finance Company: {invoice.FinanceCompany ?? "N/A"}");
            column.Item().Text($"Profit: {FormatAmount(invoice.Profit)}");
        });
    }

    private void ComposeDeliveryLegalDeclarationSection(IContainer container)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
        {
            column.Spacing(4);
            column.Item().Text("Delivery & Legal Declaration").SemiBold().FontSize(12);
            column.Item().Text($"{GetCheckbox(invoice.RcBookReceived)} RC Book received by customer");
            column.Item().Text($"{GetCheckbox(invoice.OwnershipTransferAccepted)} Ownership transfer responsibility accepted");
            column.Item().Text($"{GetCheckbox(invoice.VehicleAcceptedInAsIsCondition)} Vehicle accepted in as-is condition");
            column.Item().PaddingTop(4).Text(
                settings.TermsAndConditions ??
                "I confirm that I have received the vehicle in good condition and accepted all sale terms.");
            column.Item().PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Text("Witness: _____________________");
                row.RelativeItem()
                    .AlignRight()
                    .Text($"{settings.SignatureLine ?? "Customer Signature"}: _____________________");
            });
        });
    }

    private static void AddTableCell(TableDescriptor table, string text, bool bold = true)
    {
        var descriptor = table.Cell()
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten3)
            .PaddingVertical(4)
            .PaddingHorizontal(3)
            .Text(text);

        if (bold)
        {
            descriptor.SemiBold();
        }
    }

    private static string FormatAmount(decimal value) => $"Rs. {value:0.00}";

    private static string GetCheckbox(bool isChecked) => isChecked ? "☑" : "☐";

    private static string FormatTime(TimeSpan? deliveryTime)
    {
        if (!deliveryTime.HasValue)
        {
            return "N/A";
        }

        return DateTime.Today.Add(deliveryTime.Value).ToString("hh:mm tt");
    }
}
