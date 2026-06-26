using Microsoft.EntityFrameworkCore;
using Nexus.ApiContracts.Permissions;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Web.Auth;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.Workflow.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("WorkflowDb")
    ?? "Host=localhost;Port=5432;Database=workflow_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddNexusJwtAuth(builder.Configuration);
builder.Services.AddNexusEfCore<WorkflowDbContext>(connectionString);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<WorkflowDbContext>().Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus Workflow Service", Status = "Running" }));
app.MapGet("/health", async (WorkflowDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

app.MapPost("/api/workflow-definitions", async (CreateWorkflowDefinitionDto input, WorkflowDbContext db, CancellationToken ct) =>
{
    var definition = new WorkflowDefinition(Guid.NewGuid(), input.TenantId, input.Code, input.Name, input.Steps.ToList(), DateTimeOffset.UtcNow);
    await db.Definitions.AddAsync(definition, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/workflow-definitions/{definition.Id}", definition);
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Workflow.Manage));

app.MapGet("/api/workflow-definitions", async (WorkflowDbContext db, Guid? tenantId, CancellationToken ct) =>
{
    var query = db.Definitions.AsQueryable();
    if (tenantId.HasValue)
    {
        query = query.Where(x => x.TenantId == tenantId);
    }

    var items = await query.OrderBy(x => x.Code).ToArrayAsync(ct);
    return Results.Ok(items);
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Workflow.View));

app.MapPost("/api/workflow-instances", async (CreateWorkflowInstanceDto input, WorkflowDbContext db, CancellationToken ct) =>
{
    var definition = await db.Definitions.FindAsync([input.WorkflowDefinitionId], ct);
    if (definition is null)
    {
        return Results.NotFound(new { Code = "Workflow:DefinitionNotFound", Message = "Workflow definition was not found." });
    }

    var instance = new WorkflowInstance(Guid.NewGuid(), input.TenantId, definition.Id, input.SourceModule, input.SourceType, input.SourceId, DateTimeOffset.UtcNow);
    await db.Instances.AddAsync(instance, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/workflow-instances/{instance.Id}", instance);
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Workflow.Manage));

app.MapPost("/api/workflow-instances/{id:guid}/approve", async (Guid id, WorkflowActionDto input, WorkflowDbContext db, CancellationToken ct) =>
{
    var instance = await db.Instances.Include(x => x.Actions).SingleOrDefaultAsync(x => x.Id == id, ct);
    if (instance is null)
    {
        return Results.NotFound(new { Code = "Workflow:InstanceNotFound", Message = "Workflow instance was not found." });
    }

    instance.Approve(input.UserId, input.Comment, DateTimeOffset.UtcNow);
    await db.SaveChangesAsync(ct);
    return Results.Ok(instance);
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Workflow.Approve));

app.MapPost("/api/workflow-instances/{id:guid}/reject", async (Guid id, WorkflowActionDto input, WorkflowDbContext db, CancellationToken ct) =>
{
    var instance = await db.Instances.Include(x => x.Actions).SingleOrDefaultAsync(x => x.Id == id, ct);
    if (instance is null)
    {
        return Results.NotFound(new { Code = "Workflow:InstanceNotFound", Message = "Workflow instance was not found." });
    }

    instance.Reject(input.UserId, input.Comment, DateTimeOffset.UtcNow);
    await db.SaveChangesAsync(ct);
    return Results.Ok(instance);
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Workflow.Approve));

app.Run();

public sealed record CreateWorkflowDefinitionDto(Guid? TenantId, string Code, string Name, IReadOnlyCollection<string> Steps);
public sealed record CreateWorkflowInstanceDto(Guid? TenantId, Guid WorkflowDefinitionId, string SourceModule, string SourceType, string SourceId);
public sealed record WorkflowActionDto(Guid UserId, string? Comment);
