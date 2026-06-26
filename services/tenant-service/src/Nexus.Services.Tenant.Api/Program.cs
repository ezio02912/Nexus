using Microsoft.EntityFrameworkCore;
using Nexus.ApiContracts.Permissions;
using Nexus.BuildingBlocks.Auditing;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Messaging;
using Nexus.BuildingBlocks.Web.Auth;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.Tenant.Application.Tenants;
using Nexus.Services.Tenant.Contracts.Tenants;
using Nexus.Services.Tenant.Domain.Tenants;
using Nexus.Services.Tenant.Infrastructure.Persistence;
using Nexus.Services.Tenant.Infrastructure.Tenants;
using Nexus.SharedKernel.Auditing;
using Nexus.SharedKernel.Exceptions;
using TenantAggregate = Nexus.Services.Tenant.Domain.Tenants.Tenant;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("TenantDb")
    ?? "Host=localhost;Port=5432;Database=tenant_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddNexusJwtAuth(builder.Configuration);
builder.Services.AddNexusEfCore<TenantDbContext>(connectionString, typeof(Nexus.EventContracts.Tenants.TenantCreatedIntegrationEvent).Assembly);

if (builder.Configuration.GetValue<bool>("RabbitMq:Enabled"))
{
    builder.Services.AddNexusRabbitMqPublisher(builder.Configuration);
}

builder.Services.AddSingleton<IAuditWriter, InMemoryAuditWriter>();
builder.Services.AddScoped<ITenantRepository, EfCoreTenantRepository>();
builder.Services.AddScoped<TenantManager>();
builder.Services.AddScoped<ITenantAppService, TenantAppService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
    await db.Database.MigrateAsync();
    await SeedPlatformTenantAsync(db, app.Configuration);
    await SeedDemoTenantAsync(db, app.Configuration);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus Tenant Service", Status = "Running" }));
app.MapGet("/health", async (TenantDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

app.MapGet("/api/public/tenants/by-code/{code}", async (string code, TenantDbContext db, CancellationToken cancellationToken) =>
{
    var normalized = TenantAggregate.NormalizeCode(code);
    var tenant = await db.Tenants
        .Include(x => x.Modules)
        .Include(x => x.Settings)
        .Include(x => x.Subscription)
        .SingleOrDefaultAsync(x => x.Code == normalized, cancellationToken);

    return tenant is null
        ? Results.NotFound(new { Code = TenantErrorCodes.NotFound, Message = "Tenant was not found." })
        : Results.Ok(new TenantDto
        {
            Id = tenant.Id,
            Code = tenant.Code,
            Name = tenant.Name,
            Status = tenant.Status.ToString(),
            ConcurrencyStamp = tenant.ConcurrencyStamp,
            Modules = tenant.Modules.Select(x => new TenantModuleDto { ModuleCode = x.ModuleCode, IsEnabled = x.IsEnabled }).ToArray(),
            Settings = tenant.Settings.ToDictionary(x => x.Key, x => x.Value)
        });
});

var tenants = app.MapGroup("/api/tenants").RequireAuthorization();

tenants.MapGet("/", async (ITenantAppService appService, string? filterText, int skipCount = 0, int maxResultCount = 50, string? sorting = null, CancellationToken cancellationToken = default) =>
{
    var result = await appService.GetListAsync(new GetTenantsInput
    {
        FilterText = filterText,
        SkipCount = skipCount,
        MaxResultCount = maxResultCount,
        Sorting = sorting
    }, cancellationToken);

    return Results.Ok(result);
});

tenants.MapGet("/{id:guid}", async (Guid id, ITenantAppService appService, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await appService.GetAsync(id, cancellationToken));
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new { Code = TenantErrorCodes.NotFound, Message = "Tenant was not found." });
    }
});

tenants.MapPost("/", async (CreateTenantDto input, ITenantAppService appService, CancellationToken cancellationToken) =>
{
    try
    {
        var result = await appService.CreateAsync(input, cancellationToken);
        return Results.Created($"/api/tenants/{result.Id}", result);
    }
    catch (NexusBusinessException exception)
    {
        return Results.Conflict(new { exception.Code, exception.Message });
    }
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Tenants.Create));

tenants.MapPut("/{id:guid}/settings", async (Guid id, UpdateTenantSettingsDto input, ITenantAppService appService, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await appService.UpdateSettingsAsync(id, input, cancellationToken));
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new { Code = TenantErrorCodes.NotFound, Message = "Tenant was not found." });
    }
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Tenants.ManageSettings));

tenants.MapPost("/{id:guid}/activate", async (Guid id, ITenantAppService appService, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await appService.ActivateAsync(id, cancellationToken));
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new { Code = TenantErrorCodes.NotFound, Message = "Tenant was not found." });
    }
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Tenants.Activate));

tenants.MapPost("/{id:guid}/suspend", async (Guid id, ITenantAppService appService, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await appService.SuspendAsync(id, cancellationToken));
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new { Code = TenantErrorCodes.NotFound, Message = "Tenant was not found." });
    }
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Tenants.Activate));

tenants.MapPost("/{id:guid}/modules/enable", async (Guid id, ChangeTenantModuleDto input, ITenantAppService appService, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await appService.EnableModuleAsync(id, input, cancellationToken));
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new { Code = TenantErrorCodes.NotFound, Message = "Tenant was not found." });
    }
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Tenants.ManageModules));

tenants.MapPost("/{id:guid}/modules/disable", async (Guid id, ChangeTenantModuleDto input, ITenantAppService appService, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await appService.DisableModuleAsync(id, input, cancellationToken));
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new { Code = TenantErrorCodes.NotFound, Message = "Tenant was not found." });
    }
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Tenants.ManageModules));

app.Run();

static async Task SeedPlatformTenantAsync(TenantDbContext db, IConfiguration configuration)
{
    var tenantId = Guid.TryParse(configuration["Platform:TenantId"], out var configured)
        ? configured
        : Guid.Parse("00000000-0000-0000-0000-000000000001");

    if (await db.Tenants.IgnoreQueryFilters().AnyAsync(x => x.Id == tenantId))
    {
        return;
    }

    var platform = new TenantAggregate(tenantId, "PLATFORM", "Platform", null, DateTimeOffset.UtcNow);
    await db.Tenants.AddAsync(platform);
    await db.SaveChangesAsync();
}

static async Task SeedDemoTenantAsync(TenantDbContext db, IConfiguration configuration)
{
    if (!configuration.GetValue("DemoTenant:Enabled", true))
    {
        return;
    }

    var tenantId = Guid.TryParse(configuration["DemoTenant:TenantId"], out var configured)
        ? configured
        : Guid.Parse("00000000-0000-0000-0000-000000000100");

    var code = configuration["DemoTenant:Code"] ?? "DEMO";
    var name = configuration["DemoTenant:Name"] ?? "Demo Tenant";
    var now = DateTimeOffset.UtcNow;

    var tenant = await db.Tenants
        .Include(x => x.Modules)
        .Include(x => x.Settings)
        .IgnoreQueryFilters()
        .SingleOrDefaultAsync(x => x.Id == tenantId);

    if (tenant is null)
    {
        tenant = new TenantAggregate(tenantId, code, name, null, now);
        await db.Tenants.AddAsync(tenant);
    }

    foreach (var module in configuration.GetSection("DemoTenant:Modules").Get<string[]>() ?? ["CRM", "SALES"])
    {
        tenant.EnableModule(module, null, now);
    }

    tenant.SetSetting("Timezone", configuration["DemoTenant:Timezone"] ?? "Asia/Bangkok", null, now);
    tenant.SetSetting("Locale", configuration["DemoTenant:Locale"] ?? "vi-VN", null, now);

    await db.SaveChangesAsync();
}
