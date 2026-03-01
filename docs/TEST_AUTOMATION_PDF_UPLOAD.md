# Test Automation – PDF Generation and Upload

## Overview

- **CI-safe tests** use a **fake** `ICloudStorageService` and **fake** `IWhatsAppService`. No real Cloudinary or WhatsApp calls; no secrets.
- **Smoke tests** (optional) hit real Cloudinary when enabled via env and filter.

## Abstraction (dependency inversion)

- **ICloudStorageService** (Domain): `UploadPdfAsync(byte[] fileBytes, string fileName, CancellationToken)`. Implemented by `CloudinaryStorageService` in production and by **FakeCloudStorageService** in tests.
- **IWhatsAppService** (Application): replaced with **FakeWhatsAppService** when running PDF upload tests so send-invoice does not call Meta API.

## Tests added

| Test | What it validates |
|------|-------------------|
| **GenerateSalesPdf_ReturnsValidPdfBytes** | `GET /api/sales/{billNumber}/pdf` returns 200, `Content-Type: application/pdf`, and body bytes start with `%PDF`. |
| **SendSalesInvoice_UploadsPdf_ReturnsPdfUrl** | With fake uploader: `POST /api/sales/{billNumber}/send-invoice` returns 200, `pdfUrl` from fake; fake was called once with `fileName` containing bill number and bytes starting with `%PDF`. |
| **ManualBill_SendInvoice_UploadsPdf_ReturnsPdfUrl** | Same for manual bill: send-invoice returns `pdfUrl`; fake was called once with manual-invoice fileName and valid PDF bytes. |

## How to run locally

**All tests (no real Cloudinary):**

```bash
dotnet test
```

**Only PDF generation + upload tests (fake storage):**

```bash
dotnet test --filter "FullyQualifiedName~PdfGenerationAndUploadTests"
```

**Cloud storage smoke tests (real Cloudinary; requires env):**

Run only the tests that hit real Cloudinary (upload and fetch `pdfUrl`). Set env vars (use local env or CI secret store; **no secrets in repo**), then:

```bash
export RUN_CLOUD_SMOKE_TESTS=true
export CLOUDINARY_CLOUD_NAME="your-cloud"
export CLOUDINARY_API_KEY="your-key"
export CLOUDINARY_API_SECRET="your-secret"

dotnet test --filter "FullyQualifiedName~PdfUploadVerificationTests"
```

Those tests are tagged with `[Trait("Category", "CloudStorageSmoke")]` for CI (e.g. run only when `RUN_CLOUD_SMOKE_TESTS=true` and Cloudinary secrets are available). They validate that the returned `pdfUrl` is fetchable and returns PDF bytes (`%PDF`).

## Env vars reference

| Variable | Required for | Purpose |
|----------|--------------|---------|
| `ConnectionStrings__DefaultConnection` (or Postgres URL) | All integration tests | Database. |
| `JwtSettings__Key` (or test defaults in factory) | API tests | Auth; test factory injects test values. |
| `CLOUDINARY_CLOUD_NAME` | CloudStorageSmoke only | Real Cloudinary upload. |
| `CLOUDINARY_API_KEY` | CloudStorageSmoke only | Real Cloudinary upload. |
| `CLOUDINARY_API_SECRET` | CloudStorageSmoke only | Real Cloudinary upload. |
| `RUN_CLOUD_SMOKE_TESTS` | Optional | When `true`, run smoke tests only when you also set Cloudinary vars and use `--filter Category=CloudStorageSmoke`. |

**No secrets in repo.** Use local env, user-secrets, or CI secret store.

## Implementation details

- **FakeCloudStorageService** (Tests.Shared): implements `ICloudStorageService`, records each `UploadPdfAsync` call (fileName, bytes), returns `https://cdn.test/invoices/{publicId}.pdf`. Exposes `UploadCalls` and helpers (`BytesStartWithPdfHeader`, `FileNameContains`).
- **FakeWhatsAppService** (Tests.Shared): implements `IWhatsAppService`, no-op `SendInvoiceAsync` so send-invoice tests do not call Meta.
- **TestWebApplicationFactory**: constructor overload `(string connectionString, FakeCloudStorageService? fakeCloudStorage)`. When `fakeCloudStorage` is non-null, `ConfigureTestServices` replaces `ICloudStorageService` with the fake and `IWhatsAppService` with `FakeWhatsAppService`.
