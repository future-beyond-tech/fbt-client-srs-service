# Manual Billing – Implementation Plan

**Branch:** `feature/manual-billing`  
**Scope:** Manual billing (not tied to vehicle inventory) with manual entry, customer photo, PDF (delivery-note style), download/print/WhatsApp share, and global search by bill number.

---

## 1. Current Architecture (Confirmed)

| Layer | Project | Purpose |
|-------|---------|--------|
| **API** | `SRS.API` | Controllers, auth (`[Authorize(Roles = "Admin")]`), CORS, Swagger, DI. |
| **Application** | `SRS.Application` | DTOs, interfaces (e.g. `ISaleService`, `IInvoicePdfService`, `IWhatsAppService`, `ICustomerPhotoStorageService`), FluentValidation validators. Depends on Domain. |
| **Domain** | `SRS.Domain` | Entities (`Sale`, `Customer`, `WhatsAppMessage`, etc.), enums (`PaymentMode`), domain interfaces (e.g. `ICloudStorageService`). |
| **Infrastructure** | `SRS.Infrastructure` | EF Core (`AppDbContext`, configs, migrations), service implementations (e.g. `SaleService`, `InvoicePdfService`, `MetaWhatsAppService`, `DeliveryNotePdfGenerator`, Cloudinary/local storage). Depends on Application + Domain. |

**Existing Sales / Invoice Flow (confirmed):**

| Method | Route | Controller Action | Service |
|--------|--------|-------------------|--------|
| POST | `/api/sales` | Create | `ISaleService.CreateAsync(SaleCreateDto)` |
| GET | `/api/sales/{billNumber}` | GetByBill | `ISaleService.GetByBillNumberAsync(billNumber)` |
| GET | `/api/sales/{billNumber}/invoice` | GetInvoice | `ISaleService.GetInvoiceAsync(billNumber)` |
| POST | `/api/sales/{billNumber}/send-invoice` | SendInvoice | `ISaleService.SendInvoiceAsync(billNumber)` |

- **Bill number (Sales):** `MAX(Sales.BillNumber) + 1` in `SaleService.GenerateBillNumberAsync()`; unique index on `Sales.BillNumber`.
- **Upload:** `POST /api/upload` (multipart) → `ICustomerPhotoStorageService.SaveCustomerPhotoAsync(IFormFile)` → returns `{ url }`. Validation: 2 MB, jpeg/png/webp + magic-byte check. Dev: local `Uploads/customers/`; non-dev: Cloudinary `srs/customers`.

---

## 2. Existing Services to Reuse

| Service | Interface | Implementation | Reuse for Manual Billing |
|---------|-----------|----------------|---------------------------|
| **PDF (delivery note)** | `IInvoicePdfService`, `IPdfGenerator` | `InvoicePdfService`, `DeliveryNotePdfGenerator` | Use same **QuestPDF** layout in `InvoiceDocument` (in `SRS.Infrastructure/Services/InvoiceDocument.cs`). Input is `SaleInvoiceDto`; manual bills will supply a DTO that maps to same shape (customer + payment + optional vehicle section). |
| **PDF storage** | `ICloudStorageService` | `CloudinaryStorageService` | Same: upload PDF bytes to Cloudinary folder `invoices` (or subfolder `manual-invoices`), return URL. |
| **WhatsApp** | `IWhatsAppService` | `MetaWhatsAppService` | Same: `SendInvoiceAsync(toPhoneNumber, customerName, mediaUrl)`. Config: `WhatsApp:AccessToken`, `WhatsApp:PhoneNumberId` (env / user-secrets). |
| **Customer photo** | `ICustomerPhotoStorageService` | `LocalFileStorageService` / `CloudinaryCustomerPhotoStorageService` | Same: manual bill form uses existing `POST /api/upload` and sends `customerPhotoUrl` in create request. No new upload endpoint. |

---

## 3. DB Schema Additions

**New entity / table: `ManualBill`**

- **Id** (PK, int, identity)  
- **BillNumber** (int, unique, indexed) – sequential within manual bills only (see §4).  
- **CustomerName** (string, required)  
- **FatherName** (string, nullable)  
- **Phone** (string, required)  
- **Address** (string, nullable)  
- **PhotoUrl** (string, required) – from existing upload.  
- **IdProofNumber** (string, nullable)  
- **PaymentMode** (enum, same as Sale)  
- **CashAmount**, **UpiAmount**, **FinanceAmount**, **FinanceCompany** (nullable decimals/string)  
- **SellingPrice** (decimal) – total amount for the bill.  
- **BillDate** (DateTime)  
- **DeliveryTime** (TimeSpan, nullable)  
- **RcBookReceived**, **OwnershipTransferAccepted**, **VehicleAcceptedInAsIsCondition** (bool) – for legal section of delivery note.  
- **WitnessName** (string, nullable)  
- **Notes** (string, nullable)  
- **InvoicePdfUrl** (string, nullable)  
- **InvoiceGeneratedAt** (DateTime, nullable)  
- **CreatedAt** (DateTime, set on insert)  
- **UpdatedAt** (DateTime, set on insert/update)

**Optional:** Link to **WhatsAppMessage** for manual bills (e.g. `ManualBillId` nullable on `WhatsAppMessage`, or a separate table `ManualBillWhatsAppMessage`). Recommendation: add nullable `ManualBillId` to `WhatsAppMessage` so one message table tracks both Sale and ManualBill sends.

**Migrations:**  
- Add `ManualBills` table + `ManualBillConfiguration`.  
- Add `DbSet<ManualBill>` to `AppDbContext`.  
- If linking WhatsApp: add nullable `ManualBillId` to `WhatsAppMessage` + FK.

---

## 4. Bill Number Strategy

- **Sales:** Keep current logic: `BillNumber = MAX(Sales.BillNumber) + 1`.  
- **Manual bills:** **Separate sequence:** `BillNumber = MAX(ManualBills.BillNumber) + 1` (or 1 if empty). So sales and manual bills each have their own 1, 2, 3, …  
- **Global search:** Extend to include manual bills by bill number. Search result must distinguish source: e.g. `billType: "Sale" | "ManualBill"` and `billNumber` (and for manual, no vehicle/registration). So a search for "5" can return both Sale #5 and ManualBill #5; UI can show “Bill #5 (Sale)” vs “Bill #5 (Manual)”.

**Alternative (not recommended for this phase):** Single global sequence (e.g. table `BillNumberSequence` with next value) shared by Sales and ManualBills. Would require more schema and migration changes and could complicate existing Sale flow.

---

## 5. Endpoints for Manual Billing

| Method | Route | Purpose |
|--------|--------|--------|
| POST | `/api/manual-bills` | Create manual bill (body: manual entry + customer photo URL from existing upload). Returns 201 + DTO with `billNumber`. |
| GET | `/api/manual-bills` | List manual bills (paginated, optional filters: date range, search by customer name/phone/bill number). |
| GET | `/api/manual-bills/{billNumber}` | Get single manual bill by bill number. |
| GET | `/api/manual-bills/{billNumber}/invoice` | Get invoice data for PDF (same shape as sale invoice where applicable; vehicle section omitted or empty). Used for preview and PDF generation. |
| GET | `/api/manual-bills/{billNumber}/invoice/pdf` | Generate PDF and return file (download) or redirect to stored PDF URL. Prefer: generate-on-demand, upload to cloud, return PDF bytes with `Content-Disposition: attachment` and/or store URL on `ManualBill` for later use. |
| POST | `/api/manual-bills/{billNumber}/send-invoice` | Generate PDF (if not already), upload to cloud, send via WhatsApp using existing `IWhatsAppService`. Return same style as `SendInvoiceResponseDto` (billNumber, pdfUrl, status). |

**Notes:**

- **Upload:** No new endpoint. Frontend uses existing `POST /api/upload` for customer photo, then sends `photoUrl` in `POST /api/manual-bills`.  
- **Download/print:** Served by `GET .../invoice/pdf` (and optionally a stored `invoicePdfUrl` for caching).  
- **Share via WhatsApp:** `POST .../send-invoice`; reuse existing WhatsApp service and config.

---

## 6. Reusing PDF Template and WhatsApp

- **PDF:**  
  - Keep **`InvoiceDocument`** (QuestPDF) and **`DeliveryNotePdfGenerator`** as-is.  
  - Introduce a DTO that can represent either a Sale or a ManualBill for PDF input (e.g. `SaleInvoiceDto` with optional vehicle fields, or a new `ManualBillInvoiceDto` that is mapped to the same structure). Recommended: **extend `SaleInvoiceDto`** (or introduce an abstraction) so that vehicle fields are optional; in `InvoiceDocument`, **conditionally render Vehicle section** only when vehicle data is present. Then manual bill service builds a DTO with customer + payment + legal, and null/empty vehicle; `DeliveryNotePdfGenerator` and `InvoiceDocument` stay shared.  
  - **New service:** `IManualBillInvoicePdfService` (or extend `IInvoicePdfService` with an overload that accepts a “manual bill invoice” DTO). Implementation: build DTO from `ManualBill`, call existing `IPdfGenerator.GeneratePdfAsync(dto)`, upload bytes via `ICloudStorageService`, set `InvoicePdfUrl` / `InvoiceGeneratedAt` on `ManualBill`.  

- **WhatsApp:**  
  - Use existing `IWhatsAppService.SendInvoiceAsync(phone, customerName, mediaUrl)`. Manual bill send-invoice: get or generate PDF URL, normalize phone, call `SendInvoiceAsync`, persist `WhatsAppMessage` with `ManualBillId` if we add it.

- **File upload / storage:**  
  - Customer photo: already covered by `POST /api/upload` and `ICustomerPhotoStorageService`. No change.  
  - PDF storage: same `ICloudStorageService`; optional: subfolder `manual-invoices` to separate from sale invoices.

---

## 7. Global Search by Bill Number

- **Current:** `GET /api/search?q=...` → `ISearchService.SearchAsync(keyword)` → queries only `Sales` (bill number, customer name/phone, vehicle, date, year).  
- **Change:**  
  - Extend `SearchResultDto` with a discriminator, e.g. `BillType` or `Source`: `"Sale" | "ManualBill"`. For manual bills, `Vehicle` and `RegistrationNumber` can be empty or a placeholder.  
  - `SearchService` (or a dedicated search handler): parse keyword; if numeric, search both `Sales.BillNumber` and `ManualBills.BillNumber`; also search text on `ManualBills` (customer name, phone). Return combined list (e.g. top 50), each item with `billType` and `billNumber` so the frontend can link to `/api/sales/{id}` or `/api/manual-bills/{id}`.

---

## 8. Files / Classes to Create (Concise)

**Domain**

- `SRS.Domain/Entities/ManualBill.cs` – entity as per §3.

**Application**

- `SRS.Application/DTOs/ManualBillCreateDto.cs` – request for POST (customer details, photo URL, payment, amounts, dates, legal checkboxes, notes).  
- `SRS.Application/DTOs/ManualBillResponseDto.cs` – response for create/get (e.g. billNumber, customerName, sellingPrice, billDate).  
- `SRS.Application/DTOs/ManualBillListDto.cs` – list item (billNumber, customerName, phone, billDate, total).  
- `SRS.Application/DTOs/ManualBillInvoiceDto.cs` – invoice view for PDF (same shape as `SaleInvoiceDto` with optional vehicle; or a dedicated DTO mapped to layout).  
- `SRS.Application/Interfaces/IManualBillService.cs` – Create, GetByBillNumber, GetInvoice, GeneratePdf, SendInvoice, List (paginated).  
- `SRS.Application/Validators/ManualBillCreateDtoValidator.cs` – FluentValidation (required fields, amounts >= 0, payment consistency, no PII in messages).  
- Extend `SRS.Application/DTOs/SearchResultDto.cs` – add `BillType` (or `Source`) and optionally `Id` for linking.

**Infrastructure**

- `SRS.Infrastructure/Configurations/ManualBillConfiguration.cs` – EF configuration, index on `BillNumber`.  
- `SRS.Infrastructure/Services/ManualBillService.cs` – implements `IManualBillService`; uses `AppDbContext`, `IPdfGenerator`, `ICloudStorageService`, `IWhatsAppService`, `IDeliveryNoteSettingsService`.  
- Optional: `SRS.Infrastructure/Services/ManualBillInvoicePdfService.cs` (or integrate into `ManualBillService`) – build invoice DTO from `ManualBill`, call `IPdfGenerator`, upload, update `ManualBill`.  
- Migration: add `ManualBills` table; optionally add `ManualBillId` to `WhatsAppMessage`.  
- Update `AppDbContext`: `DbSet<ManualBill> ManualBills`.  
- Update `SearchService`: include manual bills in search; set `BillType` on results.

**API**

- `SRS.API/Controllers/ManualBillsController.cs` – POST, GET list, GET by billNumber, GET invoice, GET invoice/pdf, POST send-invoice. Same exception mapping as Sales (NotFound, BadRequest, 502 for external failures).  
- Register `IManualBillService` and new validators in DI.

**PDF / Document**

- Either: extend `InvoiceDocument` (and possibly `SaleInvoiceDto`) to support optional vehicle section (null = hide section).  
- Or: add `ManualBillInvoiceDto` and a small adapter in `DeliveryNotePdfGenerator` (or a second document type) that builds the same layout with empty vehicle section. Recommendation: **extend existing document** with optional vehicle to keep one template.

---

## 9. Security & Standards Checklist

- No secrets in repo; WhatsApp/Cloudinary via env or user-secrets.  
- No PII in logs (log bill number / IDs only, not customer name/phone in plain text).  
- Validate all inputs (FluentValidation); use ProblemDetails for API errors.  
- File upload: reuse existing 2 MB + type + magic-byte validation; no new upload surface for manual bills.  
- Secure URLs: generated PDF URLs from Cloudinary (or signed if required).  
- Auth: same `[Authorize(Roles = "Admin")]` for manual-bills endpoints.

---

## 10. Phase Estimates

| Phase | Scope | Estimate |
|-------|--------|----------|
| **Backend** | Domain entity, DTOs, validators, `ManualBillService`, PDF reuse (optional vehicle in `InvoiceDocument`), WhatsApp + storage reuse, `ManualBillsController`, DB migration, global search extension, unit + integration tests for new endpoints. | 3–5 days |
| **Frontend** | Manual Billing UI: form (manual entry + photo upload via existing API), list/filter, detail, invoice preview, download PDF, send via WhatsApp (call new endpoints). Reuse existing upload component and styling. | 2–4 days |
| **E2E** | E2E tests: create manual bill with photo, generate PDF, download, send-invoice; global search by manual bill number; optional: print/share flows. | 1–2 days |

**Total (order-of-magnitude):** 6–11 days, depending on tests and frontend complexity.

---

## 11. Reference

- **Sales API contract:** `src/API_COMPLETE_DOCUMENTATION.md` – POST /api/sales (SaleCreateDto), GET invoice (SaleInvoiceDto), POST send-invoice (SendInvoiceResponseDto).  
- **Upload:** `POST /api/upload` – multipart, returns `{ url }`.  
- **Delivery note PDF:** `src/SRS.Infrastructure/Services/InvoiceDocument.cs` – QuestPDF, sections: Header, Bill Info, Customer (photo), Vehicle, Payment, Delivery & Legal.  
- **WhatsApp:** `MetaWhatsAppService` – config keys `WhatsApp:AccessToken`, `WhatsApp:PhoneNumberId`.

---

*No code has been written in this phase; only scaffolding (e.g. empty entity/DTO/controller shells) can be added in a follow-up. All implementation will follow FBT standards (SOLID, DRY, KISS/YAGNI, Clean Architecture, FluentValidation, ProblemDetails, structured logging) and security-by-default.*
