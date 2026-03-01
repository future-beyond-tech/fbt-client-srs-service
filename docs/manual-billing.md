# Manual Billing – Documentation

Standalone bills not tied to vehicle inventory: manual entry, customer photo, delivery-note PDF, and WhatsApp send.

---

## 1. API Endpoints Summary

All under base path **`/api/manual-bills`**. Authentication: **Bearer token**, role **Admin**.

| Method | Route | Description |
|--------|--------|-------------|
| **POST** | `/api/manual-bills` | Create a manual bill. Body: customer details, `photoUrl`, `itemDescription`, `amountTotal`, payment split. Returns `billNumber`, optional `pdfUrl`, `createdAt`. |
| **GET** | `/api/manual-bills/{billNumber}` | Get full manual bill detail by bill number. |
| **GET** | `/api/manual-bills/{billNumber}/invoice` | Get invoice DTO (for PDF generation / preview). |
| **GET** | `/api/manual-bills/{billNumber}/pdf` | Get or create PDF; returns `{ pdfUrl }`. Use `?redirect=true` to redirect to the PDF URL. |
| **POST** | `/api/manual-bills/{billNumber}/send-invoice` | Generate PDF (if needed), store URL, send via WhatsApp. Returns `{ billNumber, pdfUrl, status }`. |

**Request/response examples**

- **Create (POST /api/manual-bills)**  
  Request: `customerName`, `phone` (E.164 or 10-digit), `address` (optional), `photoUrl` (from upload), `itemDescription`, `amountTotal`, `paymentMode` (1=Cash, 2=UPI, 3=Finance), `cashAmount` / `upiAmount` / `financeAmount` (must sum to `amountTotal`), `financeCompany` (optional).  
  Response `201`: `{ billNumber, pdfUrl?, createdAt }`.

- **Get PDF (GET .../pdf)**  
  Response `200`: `{ pdfUrl }`. With `?redirect=true`: `302` to storage URL.

- **Send invoice (POST .../send-invoice)**  
  Response `200`: `{ billNumber, pdfUrl, status }` (e.g. `"Sent"`).  
  Errors: `404` (bill not found), `400` (e.g. missing/invalid phone), `502` (storage or WhatsApp failure).

**Global search**

- **GET /api/search?q=**  
  Includes manual bills. Each result has **`type`**: `"Sale"` or `"ManualBill"`. For manual bills, `vehicle` and `registrationNumber` are null. Phone in results is **masked** (e.g. `******3210`).

---

## 2. Environment Variables

**No secrets in repo.** Use environment variables or dotnet user-secrets. Placeholder names only.

| Purpose | Configuration key / env var | Notes |
|--------|------------------------------|--------|
| Database | `ConnectionStrings__DefaultConnection` or `DATABASE_URL` | PostgreSQL connection string. |
| JWT | `JwtSettings__Key`, `JwtSettings__Issuer`, `JwtSettings__Audience` | Auth; key must be sufficiently long. |
| CORS | `Cors__AllowedOrigins` | JSON array of allowed frontend origins. |
| **Cloudinary (PDF & photos)** | `Cloudinary__CloudName`, `Cloudinary__ApiKey`, `Cloudinary__ApiSecret` — or env: `CLOUDINARY_CLOUD_NAME`, `CLOUDINARY_API_KEY`, `CLOUDINARY_API_SECRET`; optional `CLOUDINARY_FOLDER` | PDFs and customer photos; required for non-Development. |
| **WhatsApp (Meta)** | `WhatsApp__AccessToken`, `WhatsApp__PhoneNumberId` | Used for send-invoice; required for WhatsApp delivery. |

Example (values **must not** be committed):

```bash
# .env or host environment (placeholders – replace with real values)
ConnectionStrings__DefaultConnection="Host=localhost;Database=srs;Username=your_user;Password=your_password"
JwtSettings__Key="your-32-char-or-longer-secret-key"
Cloudinary__CloudName="your_cloud_name"
Cloudinary__ApiKey="your_api_key"
Cloudinary__ApiSecret="your_api_secret"
WhatsApp__AccessToken="your_meta_whatsapp_token"
WhatsApp__PhoneNumberId="your_phone_number_id"
```

Local dev: use **dotnet user-secrets** or a local `appsettings.Development.json` (in `.gitignore`).

For **proving PDF upload and URL validity** (curl commands, evidence steps, Cloudinary folder naming), see [PDF_UPLOAD_VERIFICATION.md](PDF_UPLOAD_VERIFICATION.md).

---

## 3. Local Dev Setup

1. **Clone and restore**
   - `git clone <repo-url>`, `cd <repo>`, `dotnet restore`.

2. **Database**
   - PostgreSQL running locally (or Docker).
   - Set `ConnectionStrings__DefaultConnection` (or `DATABASE_URL`).
   - Run migrations: from repo root,  
     `dotnet ef database update --project src/SRS.Infrastructure --startup-project src/SRS.API`.

3. **Secrets / config**
   - Set JWT key, Cloudinary, and WhatsApp placeholders (see above).  
   - Development can use **local file storage** for customer photos (no Cloudinary); PDF storage may still need Cloudinary unless stubbed.

4. **Run API**
   - `dotnet run --project src/SRS.API` (or F5).  
   - Swagger: `https://localhost:<port>/swagger`.

5. **Manual billing flow**
   - Upload customer photo: `POST /api/upload` (multipart), get `url`.
   - Create manual bill: `POST /api/manual-bills` with that `photoUrl` and other fields.
   - Get PDF URL: `GET /api/manual-bills/{billNumber}/pdf` (or `?redirect=true` to open in browser).
   - Send via WhatsApp: `POST /api/manual-bills/{billNumber}/send-invoice`.

---

## 4. Troubleshooting: “PDF failed to load in browser”

- **Cause** – PDF is served from storage (e.g. Cloudinary). Browser loads it from that origin; CORS or URL validity can break loading.

**Checks:**

1. **PDF URL**
   - Call `GET /api/manual-bills/{billNumber}/pdf` and use the returned `pdfUrl` in a new tab. If it fails, the issue is with the storage URL or its CORS/headers.

2. **Storage URL**
   - Confirm PDFs are uploaded and the returned URL is HTTPS and reachable (no broken or temporary links). Cloudinary raw/PDF URLs are typically stable and CORS-enabled for browser use.

3. **CORS**
   - Storage provider (e.g. Cloudinary) must allow your frontend origin for `GET` if the app loads the PDF in an iframe or via `fetch`. Cloudinary usually allows this; if using a custom CDN, add the frontend origin to CORS.

4. **Redirect vs embed**
   - Use `GET .../pdf?redirect=true` to send the user to the storage URL (same-origin not required). For embedding, ensure the storage domain allows your site in CORS and that the URL is used as `src` or via a link.

5. **Mixed content**
   - If the app is HTTPS, `pdfUrl` must be HTTPS; otherwise the browser may block it.

6. **Auth**
   - The API endpoint is protected; the **storage URL** is typically public read (signed or not). If the PDF URL requires auth, the browser must send credentials when loading it (e.g. in a new tab this may not happen).

---

## 5. Storage URL Rules and CORS

- **Where PDFs are stored**  
  Same storage as sales invoices (e.g. Cloudinary folder such as `invoices`). Manual bill files use a prefix (e.g. `manual-invoice-{billNumber}-...`) to avoid clashes.

- **URL shape**  
  Storage returns a **stable, public HTTPS URL** (e.g. Cloudinary secure URL). No secrets in the path; optional signed URLs if the provider supports them.

- **CORS**  
  - **API** – CORS is configured for frontend origins via `Cors__AllowedOrigins`. This affects calls to `/api/*` only.  
  - **Storage** – PDFs are loaded from the storage domain (e.g. `res.cloudinary.com`). That domain’s CORS policy must allow your frontend origin if you load the PDF via JavaScript (e.g. fetch, iframe). Cloudinary allows common origins; for a custom domain or proxy, configure CORS on the storage/CDN side.

- **Linking**  
  Prefer **direct link** (`<a href="{pdfUrl}" target="_blank">`) or **redirect** (`.../pdf?redirect=true`) so the browser opens the storage URL; then storage CORS is less critical.

---

## 6. WhatsApp Send Failure Handling

- **Typical failures**
  - Invalid or non-E.164 phone.
  - Meta WhatsApp API errors (token, phone number ID, rate limits, or template/media issues).
  - PDF not yet generated or storage unreachable (e.g. Cloudinary down).

- **API behaviour**
  - **404** – Manual bill not found.
  - **400** – Validation (e.g. missing/invalid phone). Response: ProblemDetails.
  - **502** – External failure (WhatsApp or storage). Response: ProblemDetails; no internal details or secrets.

- **Idempotency**
  - If the PDF already exists for the bill, it is reused. Repeated `POST .../send-invoice` only resends the same PDF link via WhatsApp; no duplicate PDF generation.

- **Operational checks**
  - Confirm `WhatsApp__AccessToken` and `WhatsApp__PhoneNumberId` are set and valid (Meta Business Suite / WhatsApp Business API).
  - Ensure phone is E.164 (e.g. `+919876543210`). The API normalizes 10-digit Indian numbers to `+91...`.
  - Check Meta’s status/dashboard for API or template issues; inspect logs for non-PII details (e.g. bill number, masked phone).

- **Logging**
  - Phone numbers are not logged in full; only a masked form (e.g. last four digits) is logged.

---

## 7. Production Deploy Checklist

- [ ] **Migrations**  
  Run EF migrations (e.g. `dotnet ef database update`) so `manual_bills` and any search indexes exist.

- [ ] **Environment variables**  
  Set in the host (or secrets manager):  
  - Database connection.  
  - `JwtSettings__Key` (and issuer/audience if overridden).  
  - `Cloudinary__CloudName`, `Cloudinary__ApiKey`, `Cloudinary__ApiSecret`.  
  - `WhatsApp__AccessToken`, `WhatsApp__PhoneNumberId`.  
  - `Cors__AllowedOrigins` for the production frontend origin(s).

- [ ] **No secrets in repo**  
  Confirm no real keys or passwords in `appsettings.json` or committed config; use placeholders only in docs.

- [ ] **Smoke tests**  
  - `GET /api/manual-bills/1` (or any existing bill number) → 200 or 404.  
  - `GET /api/search?q=1` → 200 and JSON array (possibly empty).  
  - `POST /api/manual-bills` with a minimal valid body (and valid auth) → 201.  
  - `GET /api/manual-bills/{billNumber}/pdf` (with existing bill) → 200 with `pdfUrl` or 404.  
  - (Optional) `POST /api/manual-bills/{billNumber}/send-invoice` on a test bill → 200 or 502; no 500 with internal details.

- [ ] **HTTPS**  
  API and frontend served over HTTPS; storage PDF URLs use HTTPS.

- [ ] **Logging**  
  Confirm logs do not contain full phone numbers or other PII; only masked identifiers and bill numbers.

---

*Manual billing reuses the same delivery-note PDF template, storage, and WhatsApp integration as sales. See `docs/MANUAL_BILL_PDF_WHATSAPP_ARCHITECTURE.md` for reuse and idempotency details.*
