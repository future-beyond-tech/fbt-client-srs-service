# PDF Upload Verification (Cloudinary)

This document proves that the backend generates invoice PDFs correctly and uploads them to Cloudinary (or configured cloud storage), returning a valid **HTTPS** URL that serves a real PDF (`%PDF` header).

## Prerequisites

- Cloudinary config must be set via **environment variables** or **user-secrets** (no secrets in repo):

  | Env var / Secret           | Required | Description        |
  |----------------------------|----------|--------------------|
  | `CLOUDINARY_CLOUD_NAME`   | Yes      | Cloud name         |
  | `CLOUDINARY_API_KEY`      | Yes      | API key            |
  | `CLOUDINARY_API_SECRET`   | Yes      | API secret         |
  | `CLOUDINARY_FOLDER`       | No       | Folder for PDFs (default: `invoices`) |

  Alternative (ASP.NET Core style): `Cloudinary__CloudName`, `Cloudinary__ApiKey`, `Cloudinary__ApiSecret`, `Cloudinary__Folder`.

- Backend uses **HTTPS** URLs only (`Api.Secure = true`); returned `pdfUrl` is always `https://res.cloudinary.com/...`.
- **Security:** API secret is never printed in logs. Mask phone numbers in any evidence (e.g. `+91****3210`).

---

## Cloudinary folder and public_id rules

| Flow         | Folder   | Public ID (filename)                    | Example URL path                          |
|-------------|----------|------------------------------------------|-------------------------------------------|
| **Sales**   | `invoices` (or `CLOUDINARY_FOLDER`) | `invoice-{billNumber}-{yyyyMMddHHmmss}`   | `invoices/invoice-42-20250228120000`       |
| **Manual**  | `invoices` (or `CLOUDINARY_FOLDER`) | `manual-invoice-{billNumber}-{yyyyMMddHHmmss}` | `invoices/manual-invoice-7-20250228120100` |

- **Folder** is configurable via `CLOUDINARY_FOLDER`; if empty, `invoices` is used.
- **Overwrite:** `Overwrite = true`, so re-generation replaces the same resource when public_id matches (timestamp makes each upload unique).
- Full URL format: `https://res.cloudinary.com/{cloud_name}/raw/upload/{folder}/{public_id}` (raw upload type).

---

## Test A: Sales flow upload

### 1. Create a sale (use existing endpoint)

You need an existing sale with a valid `billNumber`. If you don’t have one, create a sale via `POST /api/sales` (vehicle, customer, payment details, etc.) and capture `billNumber` from the response.

Example (replace `BASE_URL`, `JWT_TOKEN`, `VehicleId`, and customer details as needed):

```bash
# Create sale (adjust body for your data)
export BASE_URL="https://localhost:7xxx"
export JWT="Bearer YOUR_JWT_TOKEN"

curl -s -X POST "${BASE_URL}/api/sales" \
  -H "Authorization: ${JWT}" \
  -H "Content-Type: application/json" \
  -d '{
    "vehicleId": 1,
    "customerName": "Test Customer",
    "customerPhone": "+919876543210",
    "customerAddress": "Test Address",
    "customerPhotoUrl": "https://example.com/photo.jpg",
    "saleDate": "2025-02-28",
    "paymentMode": 1,
    "cashAmount": 100000,
    "rcBookReceived": true,
    "ownershipTransferAccepted": true,
    "vehicleAcceptedInAsIsCondition": true
  }' | jq .
```

Capture `billNumber` from the response (e.g. `42`).

### 2. Call send-invoice (server-side PDF generation + upload)

```bash
export BILL_NUMBER=42   # from step 1

curl -s -X POST "${BASE_URL}/api/sales/${BILL_NUMBER}/send-invoice" \
  -H "Authorization: ${JWT}" \
  -H "Content-Type: application/json" | jq .
```

### 3. Assert response JSON

- `pdfUrl` is non-empty.
- `status` indicates success (e.g. `"Sent"`).

**Sample response (redact phone in real evidence):**

```json
{
  "billNumber": 42,
  "pdfUrl": "https://res.cloudinary.com/your-cloud/raw/upload/invoices/invoice-42-20250228120000",
  "status": "Sent"
}
```

### 4. Validate pdfUrl is Cloudinary and HTTPS

- Domain contains `res.cloudinary.com` (or your configured host).
- URL must start with `https://`.

### 5. Fetch pdfUrl via HTTP GET

```bash
export PDF_URL="https://res.cloudinary.com/..."   # from send-invoice response

curl -sI "${PDF_URL}"
curl -s "${PDF_URL}" | head -c 20 | xxd
```

**Evidence checks:**

- Response status **200**.
- `Content-Type`: `application/pdf` (or `application/octet-stream` with valid PDF bytes).
- First bytes: `%PDF` (hex: `25 50 44 46`).

**Example terminal evidence:**

```
HTTP/2 200
content-type: application/pdf
...

00000000: 2550 4446 2d31 2e34 0a25 e2e3 cfd3 0a    %PDF-1.4.%......
```

---

## Test B: Manual bill flow upload

### 1. Create a manual bill

```bash
curl -s -X POST "${BASE_URL}/api/manual-bills" \
  -H "Authorization: ${JWT}" \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "Manual Test Customer",
    "phone": "+919876543210",
    "address": "Test Address",
    "photoUrl": "https://example.com/photo.jpg",
    "itemDescription": "Test item",
    "amountTotal": 1000,
    "paymentMode": 1,
    "cashAmount": 1000
  }' | jq .
```

Capture `billNumber` from the response.

### 2. Call send-invoice (generates PDF and uploads to Cloudinary)

```bash
export MANUAL_BILL_NUMBER=7   # from step 1

curl -s -X POST "${BASE_URL}/api/manual-bills/${MANUAL_BILL_NUMBER}/send-invoice" \
  -H "Authorization: ${JWT}" \
  -H "Content-Type: application/json" | jq .
```

### 3. Assert response JSON

- `pdfUrl` is non-empty.
- `status` indicates success (e.g. `"Sent"`).

**Sample response (redact phone in evidence):**

```json
{
  "billNumber": 7,
  "pdfUrl": "https://res.cloudinary.com/your-cloud/raw/upload/invoices/manual-invoice-7-20250228120100",
  "status": "Sent"
}
```

### 4. Validate pdfUrl and fetch PDF

Same as Test A steps 4–5: ensure domain contains `res.cloudinary.com`, URL is HTTPS, then:

```bash
curl -sI "${PDF_URL}"
curl -s "${PDF_URL}" | head -c 20 | xxd
```

- Status **200**, `Content-Type` application/pdf (or octet-stream with PDF bytes), first bytes `%PDF`.

---

## Exact curl commands (copy-paste, set vars first)

**Set once:**

```bash
export BASE_URL="https://localhost:7xxx"   # or your API base URL
export JWT="Bearer YOUR_JWT_TOKEN"
```

**Sales (A):** replace `BILL_NUMBER` with a real sale bill number.

```bash
# Send invoice
curl -s -X POST "${BASE_URL}/api/sales/BILL_NUMBER/send-invoice" \
  -H "Authorization: ${JWT}" \
  -H "Content-Type: application/json" | jq .

# Then (set PDF_URL from response):
# curl -sI "${PDF_URL}"
# curl -s "${PDF_URL}" | head -c 20 | xxd
```

**Manual bill (B):** create bill then send-invoice (set `MANUAL_BILL_NUMBER` from create response).

```bash
# Create manual bill
curl -s -X POST "${BASE_URL}/api/manual-bills" \
  -H "Authorization: ${JWT}" \
  -H "Content-Type: application/json" \
  -d '{"customerName":"C","phone":"+919876543210","address":"A","photoUrl":"https://example.com/p.jpg","itemDescription":"Item","amountTotal":100,"paymentMode":1,"cashAmount":100}' | jq .

# Send invoice (use billNumber from above)
curl -s -X POST "${BASE_URL}/api/manual-bills/MANUAL_BILL_NUMBER/send-invoice" \
  -H "Authorization: ${JWT}" \
  -H "Content-Type: application/json" | jq .

# Then (set PDF_URL from response):
# curl -sI "${PDF_URL}"
# curl -s "${PDF_URL}" | head -c 20 | xxd
```

---

## Automated verification tests

Integration tests in `tests/SRS.IntegrationTests/PdfUpload/PdfUploadVerificationTests.cs`:

- **Sales:** seed a sale → `POST /api/sales/{billNumber}/send-invoice` → assert 200, `pdfUrl` not empty, `status` present → if URL contains `res.cloudinary.com`, GET url → 200, Content-Type pdf/octet-stream, first bytes `%PDF`.
- **Manual bill:** create manual bill → `POST /api/manual-bills/{billNumber}/send-invoice` → same assertions and fetch.

When Cloudinary is not configured (e.g. test env with dummy credentials), upload may return 502/409; the tests accept that and skip the fetch step. With real Cloudinary config, the full flow is validated.

Run:

```bash
dotnet test tests/SRS.IntegrationTests/SRS.IntegrationTests.csproj --filter "FullyQualifiedName~PdfUploadVerificationTests"
```

---

## Security checklist

- [ ] Cloudinary API secret is **never** printed in logs or responses.
- [ ] Phone numbers in evidence or logs are **masked** (e.g. last four digits only).
- [ ] Config is from env vars or user-secrets; **no secrets in repo**.
