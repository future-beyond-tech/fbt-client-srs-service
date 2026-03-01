# Delivery Note PDF – What Changed

## Summary
Backend PDF generation now uses a **single branded Delivery Note template** for both **Sales** and **Manual Billing**. The PDF matches the client’s expected layout (blue header, delivery note title, seller/buyer cards, yellow ref strip, two-column details, Tamil terms, signature footer). "Address not configured" is never shown; safe defaults are used instead.

## What Changed

### 1. Safe defaults (no “Address not configured”)
- **DeliveryNoteSettingsService**: Default shop address is `"—"` instead of "Address not configured."  
- **Mapping**: All mappers use `Safe` / `SafeAddress` so null, empty, or “not configured” become `"—"` or a fallback (e.g. shop name `"SRS Billing System"`).

### 2. Single template + theme
- **DeliveryNoteTemplateViewModel**: One view model for the shared template (shop, bill meta, title, seller/buyer, greeting, ref, body, left/right tables, Tamil terms, footer).  
- **DeliveryNotePdfTheme**: Theme constants (header blue, ref/card yellow, table header blue, fonts, spacing). Used only in the template.  
- **DeliveryNotePdfTemplate**: New QuestPDF document in Infrastructure that renders the full branded layout from the view model. Takes optional customer photo and logo; if photo is missing, shows a small placeholder (no throw). Logo is shown in the header when provided.

### 3. Mappers (no business logic in template)
- **SalesInvoicePdfMapper**: Maps `SaleInvoiceDto` + `DeliveryNoteSettingsDto` → `DeliveryNoteTemplateViewModel` (vehicle wording, vehicle/payment tables, amount in words, ref from registration/brand/model).  
- **ManualBillPdfMapper**: Maps `ManualBillPdfViewModel` + `DeliveryNoteSettingsDto` → `DeliveryNoteTemplateViewModel` (item wording, item/payment tables, seller = shop, buyer from view model, greeting/ref from item).

### 4. Generator wiring
- **DeliveryNotePdfGenerator**:  
  - **Sales**: Loads settings, downloads customer photo and logo, builds VM with **SalesInvoicePdfMapper**, creates **DeliveryNotePdfTemplate**, returns `GeneratePdf()`.  
  - **Manual**: Same flow using **ManualBillPdfMapper** and manual bill view model.  
- **InvoiceDocument** and **ManualBillInvoiceDocument** are no longer used by the generator; both flows use **DeliveryNotePdfTemplate**.

### 5. Layout (matches expectation)
- Top brand header bar (shop name + taglines left, address right; optional logo).  
- Bill meta row: Bill No left, Date right.  
- Centered title block with divider; customer photo top-right (placeholder if missing).  
- Two cards: FROM (seller/shop), TO (buyer) with yellow strip headers.  
- Greeting line + yellow REF strip.  
- Body and risk paragraphs.  
- Two-column section: left = vehicle/item details table, right = payment details table.  
- Tamil terms block.  
- Footer: thank you, authorized signature (right), signature of dealer (left), signature of buyer (right).

### 6a. Tamil Terms & Conditions block (Manual Billing)
- **Position**: After the details tables (Vehicle/Payment), before “Thank you for your purchase.” and signature lines.  
- **Content source (Option A preferred)**: From configuration/DB: `TamilTermsAndConditions` on Delivery Note settings. If empty, falls back to `TermsAndConditions`; if both empty, uses **PdfContentConstants.DefaultTamilTerms** (Option B fallback).  
- **Style**: Each line rendered as a bullet (•) in **red** (`#c00`), with readable line spacing.  
- **Font**: When the Tamil font is available at startup (Noto Sans Tamil at `Infrastructure/Services/Pdf/Fonts/NotoSansTamil-Regular.ttf`), it is registered as `TamilTermsFont` and used for the Tamil block; otherwise the default font is used.  
- **Manual only**: Only the Manual Billing path sets Tamil content via **ManualBillPdfMapper**; Sales continues to use `TermsAndConditions` for `TamilTerms` (often empty).  
- **Null-safe**: Empty or null settings yield default constants; no PII in logs.

### 6. Tests
- **DeliveryNotePdfTemplateTests** (UnitTests):  
  - `GeneratePdf_ReturnsValidPdfBytes`: Asserts first bytes are `%PDF`.  
  - `GeneratePdf_WhenCustomerPhotoMissing_DoesNotThrow_AndProducesValidPdf`: Photo null → valid PDF.  
  - `GeneratePdf_WhenCustomerPhotoEmptyArray_DoesNotThrow_AndProducesValidPdf`: Photo empty array → valid PDF.  
  - `GeneratePdf_SaveToDisk_WhenEnvSet`: Optional; when `SRS_SAVE_SAMPLE_PDF=1`, writes a sample PDF to temp for review.  
  - `GeneratePdf_WithTamilTermsBlock_ReturnsValidPdf`: PDF generation succeeds when Tamil block is enabled.  
- **ManualBillPdfMapperTests**:  
  - `ToTemplateViewModel_TamilTermsAndConditions_PreferredOverTermsAndConditions`: Tamil terms from config preferred.  
  - `ToTemplateViewModel_WhenNoTamilTerms_UsesTermsAndConditions`: Fallback to TermsAndConditions.  
  - `ToTemplateViewModel_WhenBothEmpty_UsesDefaultTamilTerms`: Uses PdfContentConstants.DefaultTamilTerms.

### 7. Optional sample PDF (dev)
- Run with env: `SRS_SAVE_SAMPLE_PDF=1` and execute the unit test `GeneratePdf_SaveToDisk_WhenEnvSet`.  
- Sample PDF is written to `Path.GetTempPath()/DeliveryNote-Sample.pdf` for visual review.

## How to run tests
- **All unit tests**: `dotnet test tests/SRS.UnitTests/SRS.UnitTests.csproj`  
- **PDF template tests only**: `dotnet test tests/SRS.UnitTests/SRS.UnitTests.csproj --filter "DeliveryNotePdfTemplateTests"`  
- **Save sample PDF**: `SRS_SAVE_SAMPLE_PDF=1 dotnet test tests/SRS.UnitTests/SRS.UnitTests.csproj --filter "GeneratePdf_SaveToDisk_WhenEnvSet"`  
- **Save Manual Bill PDF (with Tamil block)**: `SRS_SAVE_MANUAL_PDF=1 dotnet test tests/SRS.UnitTests/SRS.UnitTests.csproj --filter "GeneratePdf_SaveManualBillStyleToDisk"` → writes to `{TempPath}/DeliveryNote-ManualBill-Sample.pdf`.

## Cloudinary / PDF URL
- No change to upload or content type: existing Cloudinary usage and `pdfUrl` behaviour are unchanged; the generator only returns PDF bytes as before.
