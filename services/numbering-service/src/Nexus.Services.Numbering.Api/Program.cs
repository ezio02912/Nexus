using Nexus.BuildingBlocks.Observability;
using Microsoft.EntityFrameworkCore;
using Nexus.ApiContracts.Permissions;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Web.Auth;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.Numbering.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.AddNexusObservability("numbering-service");

var connectionString = builder.Configuration.GetConnectionString("NumberingDb")
    ?? "Host=localhost;Port=5432;Database=numbering_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddNexusJwtAuth(builder.Configuration);
builder.Services.AddNexusEfCore<NumberingDbContext>(connectionString);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<NumberingDbContext>().Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus Numbering Service", Status = "Running" }));
app.MapGet("/health", async (NumberingDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

app.MapPost("/api/numbering/next", async (NextNumberRequest input, NumberingDbContext db, CancellationToken ct) =>
{
    var key = BuildKey(input.TenantId, input.Module, input.DocumentType, input.Period);

    // Atomic upsert + increment so concurrent callers never receive the same number.
    var values = await db.Database
        .SqlQueryRaw<long>(
            "INSERT INTO number_sequences (id, sequence_key, current_value) VALUES ({0}, {1}, 1) " +
            "ON CONFLICT (sequence_key) DO UPDATE SET current_value = number_sequences.current_value + 1 " +
            "RETURNING current_value AS \"Value\"",
            Guid.NewGuid(), key)
        .ToListAsync(ct);

    var next = values[0];
    var padding = input.Padding <= 0 ? 5 : input.Padding;
    var number = $"{input.Prefix}{next.ToString().PadLeft(padding, '0')}";
    return Results.Ok(new NextNumberResult(key, number, next));
}).RequireAuthorization(); // Internal allocator: any authenticated caller (incl. tenant users) may draw a number.

app.MapGet("/api/numbering/sequences", async (NumberingDbContext db, CancellationToken ct) =>
{
    var items = await db.Sequences
        .OrderBy(x => x.SequenceKey)
        .Select(x => new NumberSequenceDto(x.SequenceKey, x.CurrentValue))
        .ToArrayAsync(ct);

    return Results.Ok(items);
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Numbering.View));

app.MapNexusObservability();
app.Run();

static string BuildKey(Guid? tenantId, string module, string documentType, string? period)
{
    var effectivePeriod = period ?? DateTimeOffset.UtcNow.ToString("yyyyMM");
    return $"{tenantId?.ToString() ?? "host"}:{module.Trim().ToUpperInvariant()}:{documentType.Trim().ToUpperInvariant()}:{effectivePeriod}";
}

public sealed record NextNumberRequest(Guid? TenantId, string Module, string DocumentType, string Prefix, int Padding, string? Period);
public sealed record NextNumberResult(string SequenceKey, string Number, long Value);
public sealed record NumberSequenceDto(string Key, long CurrentValue);
