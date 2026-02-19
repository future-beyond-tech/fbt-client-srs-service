using Microsoft.EntityFrameworkCore;
using SRS.Infrastructure.Persistence;

namespace SRS.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rawConnectionString = configuration["DATABASE_URL"] 
                                  ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(rawConnectionString))
        {
            throw new InvalidOperationException("No connection string found!");
        }

// Convert if necessary
        var connectionString = MapRailwayConnectionString(rawConnectionString);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
    
    private static string MapRailwayConnectionString(string databaseUrl)
    {
        // If it doesn't start with postgres://, it's already a standard string or empty
        if (!databaseUrl.StartsWith("postgres://")) 
            return databaseUrl;

        var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':');

        return $"Host={databaseUri.Host};" +
               $"Port={databaseUri.Port};" +
               $"Database={databaseUri.AbsolutePath.TrimStart('/')};" +
               $"Username={userInfo[0]};" +
               $"Password={userInfo[1]};" +
               $"SSL Mode=Require;Trust Server Certificate=true;";
    }
}
