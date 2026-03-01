# Docker: wkhtmltopdf for invoice PDFs

**Sales** and **Manual Billing** invoice PDFs are generated using the **wkhtmltopdf CLI** only. There is **no DinkToPdf** (no native `libwkhtmltox`). The application invokes `wkhtmltopdf` via `ProcessStartInfo` and fails startup if the CLI is not available.

## Install wkhtmltopdf in the image

The project **Dockerfile** uses `mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim` and installs wkhtmltopdf in the runtime stage:

```dockerfile
RUN apt-get update \
    && apt-get install -y --no-install-recommends wkhtmltopdf \
    && rm -rf /var/lib/apt/lists/*
```

For other Debian/Ubuntu-based images, use the same `apt-get install wkhtmltopdf`. The package typically pulls required dependencies (e.g. libxrender).

### Alpine

```dockerfile
RUN apk add --no-cache wkhtmltopdf
```

## Font (Tamil)

**Noto Sans Tamil** is embedded in the HTML (base64) when the font file is present at:

- `{AppContext.BaseDirectory}/Infrastructure/Services/Pdf/Fonts/NotoSansTamil-Regular.ttf`

The API project copies this file into publish output; the Dockerfile copies publish into the image. No system font install is required for Tamil rendering. HTML includes `<meta charset="utf-8">` and wkhtmltopdf is invoked with `--encoding utf-8`.

## Behaviour at runtime

- On startup, the API runs `wkhtmltopdf --version`. If the command is **not found** or returns a non-zero exit code, the application throws **InvalidOperationException** and does **not** start.
- PDF generation writes HTML to a temp file, runs `wkhtmltopdf --encoding utf-8 --enable-local-file-access --print-media-type <input.html> <output.pdf>`, then reads the PDF bytes and deletes temp files.
- There is no fallback PDF generator. wkhtmltopdf CLI is **mandatory** for both local and production.

## Verify in container

```bash
docker compose exec api wkhtmltopdf --version
```

Then call (with valid Admin JWT):

- `GET /api/sales/{billNumber}/pdf?download=true`
- `GET /api/manual-bills/{billNumber}/pdf?download=true`

Expect: **200**, **Content-Type: application/pdf**, and first 4 bytes **%PDF**.

## Validation checklist

See **docs/PDF_DOCKER_VALIDATION.md** for the full validation checklist (build, wkhtmltopdf version, PDF download, Cloudinary, security).
