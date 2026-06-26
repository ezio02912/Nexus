using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nexus.Services.Identity.Domain.Users;

namespace Nexus.Services.Identity.Infrastructure.Persistence;

/// <summary>
/// Seeds a platform administrator account so the system is usable after a fresh migration.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(IdentityDbContext dbContext, IPasswordHasher passwordHasher, IConfiguration configuration)
    {
        var tenantId = Guid.TryParse(configuration["Platform:TenantId"], out var configured)
            ? configured
            : Guid.Parse("00000000-0000-0000-0000-000000000001");

        await SeedUserAsync(
            dbContext,
            passwordHasher,
            tenantId,
            configuration["Platform:AdminUserName"] ?? "admin",
            configuration["Platform:AdminEmail"] ?? "admin@nexus.local",
            configuration["Platform:AdminPassword"] ?? "Admin@123",
            ["ADMIN"]);

        if (configuration.GetValue("DemoTenant:Enabled", true))
        {
            var demoTenantId = Guid.TryParse(configuration["DemoTenant:TenantId"], out var configuredDemo)
                ? configuredDemo
                : Guid.Parse("00000000-0000-0000-0000-000000000100");

            await SeedUserAsync(
                dbContext,
                passwordHasher,
                demoTenantId,
                configuration["DemoTenant:AdminUserName"] ?? "tenantadmin",
                configuration["DemoTenant:AdminEmail"] ?? "tenantadmin@demo.local",
                configuration["DemoTenant:AdminPassword"] ?? "123123",
                ["TENANTADMIN"]);
        }
    }

    private static async Task SeedUserAsync(
        IdentityDbContext dbContext,
        IPasswordHasher passwordHasher,
        Guid tenantId,
        string userName,
        string email,
        string password,
        IReadOnlyCollection<string> roles)
    {
        var normalizedUserName = User.NormalizeUserName(userName);
        var existing = await dbContext.Users
            .IgnoreQueryFilters()
            .Include(x => x.Roles)
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.UserName == normalizedUserName);

        if (existing is not null)
        {
            existing.ChangePassword(passwordHasher.HashPassword(password), null, DateTimeOffset.UtcNow);
            existing.SetRoles(roles, null, DateTimeOffset.UtcNow);
            await dbContext.SaveChangesAsync();
            return;
        }

        var admin = new User(
            Guid.NewGuid(),
            tenantId,
            userName,
            email,
            passwordHasher.HashPassword(password),
            null,
            DateTimeOffset.UtcNow);

        admin.SetRoles(roles, null, DateTimeOffset.UtcNow);

        await dbContext.Users.AddAsync(admin);
        await dbContext.SaveChangesAsync();
    }
}
