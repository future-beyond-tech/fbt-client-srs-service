# Manual Bill PDF + WhatsApp Send – Architecture Notes

## Overview

Manual bills use the **same delivery note PDF template and storage/WhatsApp services** as Sales. PDF generation is **idempotent**: once a PDF is generated and stored, its URL is reused on subsequent send-invoice or GET pdf calls.

---

## Reuse

| Concern | Sales | Manual Bills | Reuse |
|--------|--------|---------------|--------|
| **PDF layout** | `InvoiceDocument` (QuestPDF) | Same | Single template: header (shop), bill info, customer + photo, vehicle/item section, payment, footer/terms. Manual bills pass `VehicleBrand="Manual"`, `VehicleModel=ItemDescription`, `RegistrationNumber="N/A"`. |
| **PDF generation** | `IPdfGenerator` / `DeliveryNotePdfGenerator` | Same | `ManualBillInvoicePdfService` builds `SaleInvoiceDto` from `ManualBill` and calls `IPdfGenerator.GeneratePdfAsync(saleInvoiceDto)`. |
| **PDF storage** | `ICloudStorageService` (Cloudinary, folder `invoices`) | Same | File name prefix `manual-invoice-{billNumber}-{timestamp}.pdf` to avoid collision with sales. |
| **WhatsApp** | `IWhatsAppService.SendInvoiceAsync(phone, customerName, mediaUrl)` | Same | Config: `WhatsApp:AccessToken`, `WhatsApp:PhoneNumberId` (env / user-secrets). |
| **Phone normalization** | E.164 (e.g. +91…) | Same | `PhoneNormalizer.NormalizeToE164`; validation in create validator. |

No new template or storage backend; only a manual-bill–specific PDF service and send-invoice handler that orchestrate the same building blocks.

---

## Idempotency

- **`IManualBillInvoicePdfService.GetOrCreatePdfUrlAsync(billNumber)`**  
  - If `ManualBill.InvoicePdfUrl` is already set, returns that URL and does **not** regenerate or re-upload.  
  - Otherwise: load bill → build DTO → generate PDF → upload → set `InvoicePdfUrl` and `InvoiceGeneratedAt` on the entity → return URL.

- **POST send-invoice**  
  - Calls `GetOrCreatePdfUrlAsync` then sends the URL via WhatsApp.  
  - Second (and later) calls for the same bill reuse the stored URL; only WhatsApp is called again.

- **GET pdf**  
  - Calls `GetOrCreatePdfUrlAsync` and returns `{ pdfUrl }` or, with `?redirect=true`, redirects to that URL.

So: **first** send-invoice or GET pdf may create and store the PDF; **subsequent** calls reuse the same URL.

---

## Security & Operations

- **Secrets:** WhatsApp and Cloudinary use env vars / user-secrets; nothing stored in repo.
- **Logging:** Phone numbers are not logged in full; `PhoneMask.MaskLastFour` is used (e.g. `******3210`).
- **Errors:** All failures surface as RFC 7807 ProblemDetails (via existing middleware/filters); no internal exception details leaked.
- **Validation:** Bill must exist; phone must be present and valid (E.164) before sending.

---

## Endpoints

| Method | Route | Behavior |
|--------|--------|----------|
| **POST** | `/api/manual-bills/{billNumber}/send-invoice` | Get-or-create PDF URL → send WhatsApp with that URL → return `{ billNumber, pdfUrl, status }`. |
| **GET** | `/api/manual-bills/{billNumber}/pdf` | Get-or-create PDF URL → return `{ pdfUrl }` (default) or 302 redirect to `pdfUrl` when `?redirect=true`. |

---

## Files Touched / Added

- **Domain:** `ManualBill` – added `InvoicePdfUrl`, `InvoiceGeneratedAt`.
- **Application:** `PhoneMask` (mask for logs), `IManualBillInvoicePdfService`, `SendManualBillInvoiceCommand` / `ISendManualBillInvoiceHandler` / `SendManualBillInvoiceHandler`, `ManualBillDetailDto.InvoicePdfUrl`.
- **Infrastructure:** `ManualBillConfiguration` (new columns), `ManualBillInvoicePdfService`, migration `AddManualBillInvoicePdfUrl`.
- **API:** `ManualBillsController` – GET `{billNumber}/pdf`, POST `{billNumber}/send-invoice`; DI registration for new services/handlers.
