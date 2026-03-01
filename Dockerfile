# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src

COPY src/SRS.API/SRS.API.csproj src/SRS.API/
COPY src/SRS.Application/SRS.Application.csproj src/SRS.Application/
COPY src/SRS.Domain/SRS.Domain.csproj src/SRS.Domain/
COPY src/SRS.Infrastructure/SRS.Infrastructure.csproj src/SRS.Infrastructure/
RUN dotnet restore src/SRS.API/SRS.API.csproj

COPY src/ src/
RUN dotnet publish src/SRS.API/SRS.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage: Debian bookworm-slim + wkhtmltopdf (mandatory for invoice PDFs)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS final
WORKDIR /app

# Install wkhtmltopdf CLI (required for Sales + Manual invoice PDF generation; no native lib dependency)
RUN apt-get update \
    && apt-get install -y --no-install-recommends wkhtmltopdf \
    && rm -rf /var/lib/apt/lists/*

# Non-root user (create and chown after copy)
RUN adduser --disabled-password --gecos "" appuser

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
EXPOSE 8080

COPY --from=build /app/publish .
RUN chown -R appuser:appuser /app
USER appuser
# NotoSansTamil is included in publish (API csproj copies to Infrastructure/Services/Pdf/Fonts/)

ENTRYPOINT ["dotnet", "SRS.API.dll"]
