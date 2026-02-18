using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Domain.Enums;

namespace SRS.Infrastructure.Persistence;

public static class DbInitializer
{
    private const string DefaultAdminUsername = "admin";
    private const string DefaultAdminPassword = "Admin@123";

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await context.Database.MigrateAsync();
        await SeedAdminAsync(context, passwordHasher);
    }

    public static async Task SeedAdminAsync(
        AppDbContext context,
        IPasswordHasher passwordHasher)
    {
        var adminExists = await context.Users
            .AnyAsync(u => u.Role == UserRole.Admin || u.Username == DefaultAdminUsername);

        if (!adminExists)
        {
            var admin = new User
            {
                Username = DefaultAdminUsername,
                PasswordHash = passwordHasher.Hash(DefaultAdminPassword),
                Role = UserRole.Admin
            };

            await context.Users.AddAsync(admin);
            await context.SaveChangesAsync();
        }
    }
}
