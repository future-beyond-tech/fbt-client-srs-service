using Microsoft.EntityFrameworkCore;
using SRS.Infrastructure.Persistence;

namespace SRS.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

        string? connectionString;

        if (string.IsNullOrEmpty(databaseUrl))
        {
            // Local development
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        else
        {
            // Parse Railway's URI format
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');

            connectionString = $"Host={databaseUri.Host};" +
                               $"Port={databaseUri.Port};" +
                               $"Database={databaseUri.AbsolutePath.TrimStart('/')};" +
                               $"Username={userInfo[0]};" +
                               $"Password={userInfo[1]};" +
                               $"SSL Mode=Require;Trust Server Certificate=true;";
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
        return services;
    }
}
