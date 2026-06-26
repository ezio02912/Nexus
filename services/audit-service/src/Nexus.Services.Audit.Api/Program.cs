using Nexus.BuildingBlocks.Observability;
using Microsoft.EntityFrameworkCore;
using Nexus.ApiContracts.Permissions;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Web.Auth;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.Audit.Api.Persistence;
using Nexus.SharedKernel.Auditing;

var builder = WebApplication.CreateBuilder(args);
builder.AddNexusObservability("audit-service");

var connectionString = builder.Configuration.GetConnectionString("AuditDb")
    ?? "Host=localhost;Port=5432;Database=audit_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddNexusJwtAuth(builder.Configuration);
builder.Services.AddNexusEfCore<AuditDbContext>(connectionString);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<AuditDbContext>().Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus Audit Service", Status = "Running" }));
app.MapGet("/health", async (AuditDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

app.MapGet("/api/audit-logs", async (AuditDbContext db, Guid? tenantId, string? entityName, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
{
    var query = db.AuditLogs.AsQueryable();
    if (tenantId.HasValue)
    {
        query = query.Where(x => x.TenantId == tenantId);
    }

    if (!string.IsNullOrWhiteSpace(entityName))
    {
        query = query.Where(x => x.EntityName == entityName);
    }

    var total = await query.LongCountAsync(ct);
    var items = await query
        .OrderByDescending(x => x.OccurredAt)
        .Skip(skipCount)
        .Take(maxResultCount)
        .ToArrayAsync(ct);

    return Results.Ok(new { TotalCount = total, Items = items });
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Audit.View));

app.MapPost("/api/audit-logs", async (CreateAuditLogDto input, AuditDbContext db, CancellationToken ct) =>
{
    var entry = new AuditLog(
        Guid.NewGuid(),
        input.TenantId,
        input.UserId,
        input.ServiceName,
        input.EntityName,
        input.EntityId,
        input.Action,
        input.Summary,
        input.CorrelationId,
        DateTimeOffset.UtcNow);

    await db.AuditLogs.AddAsync(entry, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/audit-logs/{entry.Id}", entry);
}).RequireAuthorization();

app.MapNexusObservability();
app.Run();

public sealed record CreateAuditLogDto(
    Guid? TenantId,
    Guid? UserId,
    string ServiceName,
    string EntityName,
    string? EntityId,
    AuditAction Action,
    string? Summary,
    string? CorrelationId);
