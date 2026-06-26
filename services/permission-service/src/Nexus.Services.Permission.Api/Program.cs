using Nexus.BuildingBlocks.Observability;
using Microsoft.EntityFrameworkCore;
using Nexus.ApiContracts.Permissions;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Web.Auth;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.Permission.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.AddNexusObservability("permission-service");

var connectionString = builder.Configuration.GetConnectionString("PermissionDb")
    ?? "Host=localhost;Port=5432;Database=permission_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddNexusJwtAuth(builder.Configuration);
builder.Services.AddNexusEfCore<PermissionDbContext>(connectionString);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PermissionDbContext>();
    await db.Database.MigrateAsync();
    await SeedAsync(db);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus Permission Service", Status = "Running" }));
app.MapGet("/health", async (PermissionDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

app.MapGet("/api/permissions", () => Results.Ok(NexusPermissions.All));

// Returns the permission list for a role. Kept anonymous so the identity service can resolve
// permissions during login (before a token exists).
app.MapGet("/api/roles/{roleName}/permissions", async (string roleName, PermissionDbContext db, CancellationToken ct) =>
{
    var key = Normalize(roleName);
    var permissions = await db.RolePermissions
        .Where(x => x.RoleName == key)
        .Select(x => x.Permission)
        .OrderBy(x => x)
        .ToArrayAsync(ct);

    return Results.Ok(new RolePermissionDto(key, permissions));
});

app.MapPut("/api/roles/{roleName}/permissions", async (string roleName, UpdateRolePermissionsDto input, PermissionDbContext db, CancellationToken ct) =>
{
    var key = Normalize(roleName);
    var requested = input.Permissions.Where(NexusPermissions.All.Contains).Distinct().ToArray();

    var existing = await db.RolePermissions.Where(x => x.RoleName == key).ToListAsync(ct);
    db.RolePermissions.RemoveRange(existing);
    foreach (var permission in requested)
    {
        await db.RolePermissions.AddAsync(new RolePermission(Guid.NewGuid(), key, permission), ct);
    }

    await db.SaveChangesAsync(ct);
    return Results.Ok(new RolePermissionDto(key, requested.Order().ToArray()));
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Permissions.Manage));

app.MapPost("/api/permissions/check", async (CheckPermissionDto input, PermissionDbContext db, CancellationToken ct) =>
{
    var roles = input.Roles.Select(Normalize).ToArray();
    var granted = await db.RolePermissions
        .AnyAsync(x => roles.Contains(x.RoleName) && x.Permission == input.Permission, ct);

    return Results.Ok(new PermissionCheckResultDto(input.Permission, granted));
});

app.MapNexusObservability();
app.Run();

static string Normalize(string value) => value.Trim().ToUpperInvariant();

static async Task SeedAsync(PermissionDbContext db)
{
    const string adminRole = "ADMIN";
    await SeedRoleAsync(db, adminRole, NexusPermissions.All);
    await SeedRoleAsync(db, "TENANTADMIN", NexusPermissions.All);
    await SeedRoleAsync(db, "00000000000000000000000000000100:TENANTADMIN", NexusPermissions.All);
    await db.SaveChangesAsync();
}

static async Task SeedRoleAsync(PermissionDbContext db, string roleName, IEnumerable<string> permissions)
{
    var key = Normalize(roleName);
    var existing = await db.RolePermissions
        .Where(x => x.RoleName == key)
        .Select(x => x.Permission)
        .ToArrayAsync();

    foreach (var permission in permissions.Except(existing))
    {
        await db.RolePermissions.AddAsync(new RolePermission(Guid.NewGuid(), key, permission));
    }
}

public sealed record UpdateRolePermissionsDto(IReadOnlyCollection<string> Permissions);
public sealed record RolePermissionDto(string RoleName, IReadOnlyCollection<string> Permissions);
public sealed record CheckPermissionDto(IReadOnlyCollection<string> Roles, string Permission);
public sealed record PermissionCheckResultDto(string Permission, bool IsGranted);
