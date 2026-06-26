using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.Auditing;
using Nexus.BuildingBlocks.Web.Auth;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Messaging;
using Nexus.Services.Identity.Application.Onboarding;
using Nexus.Services.Identity.Application.Users;
using Nexus.Services.Identity.Contracts.Onboarding;
using Nexus.Services.Identity.Contracts.Users;
using Nexus.Services.Identity.Domain.Onboarding;
using Nexus.Services.Identity.Domain.Users;
using Nexus.Services.Identity.Infrastructure.Onboarding;
using Nexus.Services.Identity.Infrastructure.Persistence;
using Nexus.Services.Identity.Infrastructure.Users;
using Nexus.SharedKernel.Auditing;
using Nexus.SharedKernel.Exceptions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("IdentityDb")
    ?? "Host=localhost;Port=5432;Database=identity_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddNexusJwtAuth(builder.Configuration);
builder.Services.AddNexusEfCore<IdentityDbContext>(connectionString, typeof(Nexus.EventContracts.Identity.UserCreatedIntegrationEvent).Assembly);

if (builder.Configuration.GetValue<bool>("RabbitMq:Enabled"))
{
    builder.Services.AddNexusRabbitMqPublisher(builder.Configuration);
}

builder.Services.AddSingleton<IAuditWriter, InMemoryAuditWriter>();
builder.Services.AddScoped<IUserRepository, EfCoreUserRepository>();
builder.Services.AddScoped<IOnboardingRepository, EfCoreOnboardingRepository>();
builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<UserManager>();
builder.Services.AddScoped<IUserAppService, UserAppService>();
builder.Services.AddScoped<IOnboardingAppService, OnboardingAppService>();
builder.Services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<ITenantServiceClient, HttpTenantServiceClient>();

var permissionServiceUrl = builder.Configuration["Services:Permission"] ?? "http://localhost:7203";
builder.Services.AddHttpClient<IUserPermissionResolver, HttpUserPermissionResolver>(client =>
    client.BaseAddress = new Uri(permissionServiceUrl));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await db.Database.MigrateAsync();
    await IdentitySeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<IPasswordHasher>(), app.Configuration);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus Identity Service", Status = "Running" }));
app.MapGet("/health", async (IdentityDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

var users = app.MapGroup("/api/users").RequireAuthorization();

users.MapGet("/", async (IUserAppService appService, Guid? tenantId, string? filterText, int skipCount = 0, int maxResultCount = 50, string? sorting = null, CancellationToken cancellationToken = default) =>
{
    var result = await appService.GetListAsync(new GetUsersInput
    {
        TenantId = tenantId,
        FilterText = filterText,
        SkipCount = skipCount,
        MaxResultCount = maxResultCount,
        Sorting = sorting
    }, cancellationToken);

    return Results.Ok(result);
}).RequireAuthorization(NexusPolicies.Permission(Nexus.ApiContracts.Permissions.NexusPermissions.Users.Default));

users.MapGet("/{id:guid}", async (Guid id, IUserAppService appService, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await appService.GetAsync(id, cancellationToken));
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new { Code = UserErrorCodes.NotFound, Message = "User was not found." });
    }
});

users.MapPost("/", async (CreateUserDto input, IUserAppService appService, CancellationToken cancellationToken) =>
{
    try
    {
        var result = await appService.CreateAsync(input, cancellationToken);
        return Results.Created($"/api/users/{result.Id}", result);
    }
    catch (NexusBusinessException exception)
    {
        return Results.Conflict(new { exception.Code, exception.Message });
    }
}).RequireAuthorization(NexusPolicies.Permission(Nexus.ApiContracts.Permissions.NexusPermissions.Users.Create));

users.MapPost("/{id:guid}/roles", async (Guid id, ChangeUserRolesDto input, IUserAppService appService, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await appService.ChangeRolesAsync(id, input, cancellationToken));
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new { Code = UserErrorCodes.NotFound, Message = "User was not found." });
    }
}).RequireAuthorization(NexusPolicies.Permission(Nexus.ApiContracts.Permissions.NexusPermissions.Users.ManageRoles));

app.MapPost("/api/auth/login", async (LoginDto input, IUserAppService appService, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await appService.LoginAsync(input, cancellationToken));
    }
    catch (NexusBusinessException)
    {
        return Results.Unauthorized();
    }
});

app.MapPost("/api/auth/refresh", async (RefreshTokenDto input, IUserAppService appService, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await appService.RefreshAsync(input, cancellationToken));
    }
    catch (NexusBusinessException)
    {
        return Results.Unauthorized();
    }
});

app.MapPost("/api/auth/google", async (GoogleAuthDto input, IOnboardingAppService appService, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await appService.GoogleAuthAsync(input, cancellationToken));
    }
    catch (Exception exception) when (exception is InvalidOperationException or NexusBusinessException)
    {
        return Results.BadRequest(new { Message = exception.Message });
    }
});

app.MapPost("/api/auth/login-email", async (LoginEmailDto input, IOnboardingAppService appService, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await appService.LoginEmailAsync(input, cancellationToken));
    }
    catch (NexusBusinessException)
    {
        return Results.Unauthorized();
    }
});

app.MapGet("/api/auth/me/tenant", async (string email, IOnboardingAppService appService, CancellationToken cancellationToken) =>
{
    var result = await appService.GetTenantByEmailAsync(email, cancellationToken);
    return result is null ? Results.NotFound() : Results.Ok(result);
});

app.MapPost("/api/onboarding/preview-code", async (PreviewTenantCodeDto input, IOnboardingAppService appService, CancellationToken cancellationToken) =>
    Results.Ok(await appService.PreviewCodeAsync(input, cancellationToken)));

app.MapPost("/api/onboarding/complete", async (CompleteOnboardingDto input, IOnboardingAppService appService, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await appService.CompleteAsync(input, cancellationToken));
    }
    catch (NexusBusinessException exception)
    {
        return Results.Conflict(new { exception.Code, exception.Message });
    }
});

app.Run();
