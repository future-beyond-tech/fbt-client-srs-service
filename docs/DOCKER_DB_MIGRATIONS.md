# Docker DB: Start Database and Run Migrations

Use this when your database runs in Docker (e.g. via `docker-compose.yml`) and you want to bring the DB up and apply EF Core migrations.

---

## 1. Start the PostgreSQL container

From the repo root:

```bash
docker compose up -d postgres
```

Wait until Postgres is healthy (a few seconds). The compose file maps **host port 5433** to container port 5432 so you can connect from your machine.

---

## 2. Connection string from your machine

From the **host** (where you run `dotnet ef`), use **port 5433** and the same user/database/password as in your `.env`:

| Item        | In Docker (api service) | From host (migrations)   |
|------------|--------------------------|---------------------------|
| Host       | `postgres`               | `localhost`               |
| Port       | `5432`                   | **`5433`**                |
| Database   | `${POSTGRES_DB:-srs}`     | same (e.g. `SRSDb`/`srs`) |
| User       | `${POSTGRES_USER:-srs}`   | same (e.g. `postgres`)    |
| Password   | `${POSTGRES_PASSWORD}`    | same                      |

Example (match your `.env` values):

```bash
# Linux/macOS – one line
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5433;Database=SRSDb;Username=postgres;Password=12345678"

# Or inline for a single command:
ConnectionStrings__DefaultConnection="Host=localhost;Port=5433;Database=SRSDb;Username=postgres;Password=YOUR_PASSWORD" dotnet ef database update --project src/SRS.Infrastructure --startup-project src/SRS.API
```

Replace `SRSDb`, `postgres`, and the password with the values from your `.env` (`POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`).

---

## 3. Run migrations

With the connection string set to **localhost:5433** and Postgres running:

```bash
dotnet ef database update --project src/SRS.Infrastructure --startup-project src/SRS.API
```

Ensure the **EF Core tools** are installed:

```bash
dotnet tool install --global dotnet-ef
# or update: dotnet tool update --global dotnet-ef
```

---

## 4. Full stack (API + DB)

To run both the API and the database with Docker:

```bash
docker compose up -d
```

The API container uses the internal hostname `postgres` and port `5432`; migrations are still run from the host using **localhost:5433** as above (with the same DB name/user/password).

---

## Quick reference

| Goal                    | Command |
|-------------------------|--------|
| Start only DB           | `docker compose up -d postgres` |
| Run migrations (host)    | Set `ConnectionStrings__DefaultConnection` to `Host=localhost;Port=5433;...` then `dotnet ef database update --project src/SRS.Infrastructure --startup-project src/SRS.API` |
| Start API + DB          | `docker compose up -d` |
| View DB logs            | `docker compose logs -f postgres` |
| Stop                    | `docker compose down` |
