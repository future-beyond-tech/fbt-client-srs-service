# PDF + Docker validation checklist

Single-engine invoice PDF (wkhtmltopdf **CLI**). No DinkToPdf; no fallback. App fails startup if wkhtmltopdf is unavailable.

---

## 1) Build and run

```bash
docker compose up --build
```

- API and Postgres start; API listens on port 8080.
- If wkhtmltopdf CLI is missing in the image, the API will throw at startup:  
  `InvalidOperationException: wkhtmltopdf not found. Invoice engine unavailable.`

---

## 2) wkhtmltopdf inside container

```bash
docker compose exec api wkhtmltopdf --version
```

- Expect: version string (e.g. `wkhtmltopdf 0.12.6`). Exit code 0.

---

## 3) PDF download – Sales

Obtain a valid JWT (Admin) and an existing sale `billNumber`.

```bash
curl -s -o sales-invoice.pdf -w "%{http_code}\n%{content_type}\n" \
  -H "Authorization: Bearer YOUR_JWT" \
  "http://localhost:8080/api/sales/{billNumber}/pdf?download=true"
```

- Status: **200**
- Content-Type: **application/pdf**
- File: first 4 bytes are **%PDF**
- Content-Disposition: `attachment; filename="invoice-{billNumber}.pdf"`

```bash
head -c 4 sales-invoice.pdf | xxd
# Expect: 2550 4446  (%PDF)
```

---

## 4) PDF download – Manual bills

Obtain a valid JWT and an existing manual bill `billNumber`.

```bash
curl -s -o manual-invoice.pdf -w "%{http_code}\n%{content_type}\n" \
  -H "Authorization: Bearer YOUR_JWT" \
  "http://localhost:8080/api/manual-bills/{billNumber}/pdf?download=true"
```

- Status: **200**
- Content-Type: **application/pdf**
- First 4 bytes: **%PDF**
- Content-Disposition: `attachment; filename="manual-invoice-{billNumber}.pdf"`

---

## 5) Cloudinary URL in browser

- Create a sale or manual bill, then call **POST** `.../send-invoice` (or generate PDF so the app uploads to Cloudinary).
- Response includes `pdfUrl`.
- Open `pdfUrl` in a browser: PDF loads (no “failed to load”).
- Optional: run Cloudinary smoke only when `RUN_CLOUD_SMOKE_TESTS=true` and Cloudinary env vars are set.

---

## 6) Security

- **AuthZ:** PDF endpoints require Admin role; tenant-safe access enforced.
- **HTML:** All user-provided values are HTML-encoded in the delivery note template.
- **Images:** Photo URLs are fetched server-side and embedded as base64 to avoid SSRF.
- **Logs:** Phone numbers are masked (e.g. `PhoneMask.MaskLastFour`).
- **Secrets:** No secrets in repo; use env / user-secrets.

---

## 7) Sample PDFs

- **Sales:** Create a sale → `GET /api/sales/{billNumber}/pdf?download=true` → save as `invoice-{billNumber}.pdf`.
- **Manual:** Create a manual bill → `GET /api/manual-bills/{billNumber}/pdf?download=true` → save as `manual-invoice-{billNumber}.pdf`.

Both should open in a viewer and show the delivery note layout (blue header, SELLER/BUYER cards, Tamil terms when configured, payment checkboxes).

---

## 8) Integration tests

Integration tests start the API (WebApplicationFactory). Because wkhtmltopdf **CLI** is **mandatory**, the API will not start without it. To run integration tests:

- **Option A:** Run tests inside a container or on a host where wkhtmltopdf is installed (e.g. same Docker image used for the API).
- **Option B:** Install wkhtmltopdf locally (e.g. `apt-get install wkhtmltopdf` on Debian/Ubuntu, or `brew install wkhtmltopdf` on macOS if available).

If wkhtmltopdf is not available, the test host will throw during app startup and tests will not run.
