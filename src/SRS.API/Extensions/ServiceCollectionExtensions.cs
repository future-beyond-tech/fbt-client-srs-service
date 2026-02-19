using Microsoft.EntityFrameworkCore;
using SRS.Infrastructure.Persistence;

namespace SRS.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
// If DefaultConnection is null (which it is on Railway), look for DATABASE_URL
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = configuration["DATABASE_URL"];
        }
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");
        }

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}
