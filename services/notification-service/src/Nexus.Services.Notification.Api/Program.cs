using Microsoft.EntityFrameworkCore;
using Nexus.ApiContracts.Permissions;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Web.Auth;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.Notification.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("NotificationDb")
    ?? "Host=localhost;Port=5432;Database=notification_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddNexusJwtAuth(builder.Configuration);
builder.Services.AddNexusEfCore<NotificationDbContext>(connectionString);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<NotificationDbContext>().Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus Notification Service", Status = "Running" }));
app.MapGet("/health", async (NotificationDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

var notifications = app.MapGroup("/api/notifications").RequireAuthorization();

notifications.MapGet("/", async (NotificationDbContext db, Guid? tenantId, Guid? recipientUserId, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
{
    var query = db.Notifications.AsQueryable();
    if (tenantId.HasValue)
    {
        query = query.Where(x => x.TenantId == tenantId);
    }

    if (recipientUserId.HasValue)
    {
        query = query.Where(x => x.RecipientUserId == recipientUserId);
    }

    var total = await query.LongCountAsync(ct);
    var items = await query
        .OrderByDescending(x => x.CreatedAt)
        .Skip(skipCount)
        .Take(maxResultCount)
        .ToArrayAsync(ct);

    return Results.Ok(new { TotalCount = total, Items = items });
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Notifications.View));

notifications.MapPost("/", async (CreateNotificationDto input, NotificationDbContext db, CancellationToken ct) =>
{
    var notification = new Notification(
        Guid.NewGuid(),
        input.TenantId,
        input.RecipientUserId,
        string.IsNullOrWhiteSpace(input.Channel) ? "InApp" : input.Channel,
        input.Subject,
        input.Body,
        DateTimeOffset.UtcNow);

    await db.Notifications.AddAsync(notification, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/notifications/{notification.Id}", notification);
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Notifications.Send));

notifications.MapPost("/{id:guid}/read", async (Guid id, NotificationDbContext db, CancellationToken ct) =>
{
    var notification = await db.Notifications.FindAsync([id], ct);
    if (notification is null)
    {
        return Results.NotFound();
    }

    notification.MarkRead(DateTimeOffset.UtcNow);
    await db.SaveChangesAsync(ct);
    return Results.Ok(notification);
});

app.Run();

public sealed record CreateNotificationDto(Guid? TenantId, Guid? RecipientUserId, string Channel, string Subject, string Body);
