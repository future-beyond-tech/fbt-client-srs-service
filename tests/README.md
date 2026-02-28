# SRS Tests

Unit and integration tests for the Sales & Revenue System (SRS) API.

## Prerequisites

- **.NET 8 SDK**
- **Docker** (required for integration tests; Testcontainers starts a Postgres container)

## Running tests

From the **repository root** (where the solution file is):

### Unit tests only (no Docker)

```bash
dotnet test tests/SRS.UnitTests/SRS.UnitTests.csproj
```

### Integration tests only (requires Docker)

```bash
dotnet test tests/SRS.IntegrationTests/SRS.IntegrationTests.csproj
```

### All tests

```bash
dotnet test fbt-client-srs-service.sln
```

To run only unit or only integration when using the solution:

```bash
dotnet test fbt-client-srs-service.sln --filter "FullyQualifiedName~UnitTests"
dotnet test fbt-client-srs-service.sln --filter "FullyQualifiedName~IntegrationTests"
```

## Structure

- **`SRS.UnitTests`** – Validators, business logic, no external services. Uses xUnit + FluentAssertions; Moq only when needed.
- **`SRS.IntegrationTests`** – API contract tests using `WebApplicationFactory`, Testcontainers Postgres, and test auth (no secrets).
- **`SRS.Tests.Shared`** – Shared test infrastructure: `TestWebApplicationFactory`, `TestAuthHandler`, `TestDataSeeder`, HTTP client auth extensions.

## Security and data

- **No secrets** are stored in the repo. Tests use in-memory config (e.g. test JWT key, dummy Cloudinary values).
- **No PII** in logs. Test data uses dummy values (e.g. placeholder phone numbers, “Test Customer”).
- Integration tests use a **Test** auth scheme (header `X-Test-Role: Admin` or `User`) so no real JWT or credentials are required in test code.

## CI

- Run `dotnet test fbt-client-srs-service.sln` in CI.
- Ensure Docker is available for integration tests (e.g. Docker-in-Docker or host Docker).
- Timeouts are default; for slow CI, consider increasing test timeout or reusing a single Postgres container per pipeline.
