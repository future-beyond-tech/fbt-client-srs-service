using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SRS.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = ResolveConnectionString();

        optionsBuilder.UseNpgsql(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }

    private static string ResolveConnectionString()
    {
        var directConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrWhiteSpace(directConnection))
        {
            return directConnection;
        }

        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrWhiteSpace(databaseUrl))
        {
            return "Host=localhost;Port=5432;Database=srs;Username=postgres;Password=postgres;SSL Mode=Disable;";
        }

        if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var databaseUri))
        {
            return databaseUrl;
        }

        var userInfo = databaseUri.UserInfo.Split(':');
        if (userInfo.Length < 2)
        {
            return databaseUrl;
        }

        return $"Host={databaseUri.Host};" +
               $"Port={databaseUri.Port};" +
               $"Database={databaseUri.AbsolutePath.TrimStart('/')};" +
               $"Username={userInfo[0]};" +
               $"Password={userInfo[1]};" +
               "SSL Mode=Require;Trust Server Certificate=true;";
    }
}
