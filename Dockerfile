FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/SRS.API/SRS.API.csproj src/SRS.API/
COPY src/SRS.Application/SRS.Application.csproj src/SRS.Application/
COPY src/SRS.Domain/SRS.Domain.csproj src/SRS.Domain/
COPY src/SRS.Infrastructure/SRS.Infrastructure.csproj src/SRS.Infrastructure/
RUN dotnet restore src/SRS.API/SRS.API.csproj

COPY src/ src/
RUN dotnet publish src/SRS.API/SRS.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SRS.API.dll"]
