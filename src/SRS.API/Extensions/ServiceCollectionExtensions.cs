using Microsoft.EntityFrameworkCore;
using Npgsql;
using SRS.Infrastructure.Persistence;

namespace SRS.API.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var baseConnectionString = configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException(
                                       "Connection string 'DefaultConnection' is required.");

        var databaseProvider = configuration["Database:Provider"] ?? "Npgsql";

        if (string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(baseConnectionString));
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(baseConnectionString));
        }

        return services;
    }

}