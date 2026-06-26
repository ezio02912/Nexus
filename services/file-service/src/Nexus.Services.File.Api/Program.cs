using Microsoft.EntityFrameworkCore;
using Nexus.ApiContracts.Permissions;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Web.Auth;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.File.Api.Persistence;
using Nexus.Services.File.Api.Storage;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("FileDb")
    ?? "Host=localhost;Port=5432;Database=file_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddNexusJwtAuth(builder.Configuration);
builder.Services.AddNexusEfCore<FileDbContext>(connectionString);
builder.Services.AddSingleton<IFileStorage, FileSystemFileStorage>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<FileDbContext>().Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus File Service", Status = "Running" }));
app.MapGet("/health", async (FileDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

var files = app.MapGroup("/api/files").RequireAuthorization();

files.MapGet("/", async (FileDbContext db, Guid? tenantId, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
{
    var query = db.Files.AsQueryable();
    if (tenantId.HasValue)
    {
        query = query.Where(x => x.TenantId == tenantId);
    }

    var total = await query.LongCountAsync(ct);
    var items = await query
        .OrderByDescending(x => x.CreatedAt)
        .Skip(skipCount)
        .Take(maxResultCount)
        .Select(x => new FileDto(x.Id, x.TenantId, x.FileName, x.ContentType, x.Size, x.StoragePath, x.CreatedAt))
        .ToArrayAsync(ct);

    return Results.Ok(new { TotalCount = total, Items = items });
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Files.View));

files.MapGet("/{id:guid}", async (Guid id, FileDbContext db, CancellationToken ct) =>
{
    var file = await db.Files.FindAsync([id], ct);
    return file is null
        ? Results.NotFound()
        : Results.Ok(new FileDto(file.Id, file.TenantId, file.FileName, file.ContentType, file.Size, file.StoragePath, file.CreatedAt));
});

// Metadata-only registration (kept for backward compatibility).
files.MapPost("/", async (CreateFileDto input, FileDbContext db, CancellationToken ct) =>
{
    var file = new FileObject(Guid.NewGuid(), input.TenantId, input.FileName, input.ContentType, input.Size, input.StoragePath, DateTimeOffset.UtcNow);
    await db.Files.AddAsync(file, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/files/{file.Id}", new FileDto(file.Id, file.TenantId, file.FileName, file.ContentType, file.Size, file.StoragePath, file.CreatedAt));
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Files.Upload));

// Real binary upload.
files.MapPost("/upload", async (HttpRequest request, FileDbContext db, IFileStorage storage, Guid? tenantId, CancellationToken ct) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest(new { Message = "Expected multipart/form-data." });
    }

    var form = await request.ReadFormAsync(ct);
    var formFile = form.Files.FirstOrDefault();
    if (formFile is null)
    {
        return Results.BadRequest(new { Message = "No file uploaded." });
    }

    var fileId = Guid.NewGuid();
    await using var stream = formFile.OpenReadStream();
    var storagePath = await storage.SaveAsync(fileId, formFile.FileName, stream, ct);

    var file = new FileObject(fileId, tenantId, formFile.FileName, formFile.ContentType ?? "application/octet-stream", formFile.Length, storagePath, DateTimeOffset.UtcNow);
    await db.Files.AddAsync(file, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/files/{file.Id}", new FileDto(file.Id, file.TenantId, file.FileName, file.ContentType, file.Size, file.StoragePath, file.CreatedAt));
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Files.Upload)).DisableAntiforgery();

files.MapGet("/{id:guid}/content", async (Guid id, FileDbContext db, IFileStorage storage, CancellationToken ct) =>
{
    var file = await db.Files.FindAsync([id], ct);
    if (file is null)
    {
        return Results.NotFound();
    }

    var stream = await storage.OpenAsync(file.StoragePath, ct);
    return stream is null ? Results.NotFound() : Results.File(stream, file.ContentType, file.FileName);
});

app.MapPost("/api/file-links", async (CreateFileLinkDto input, FileDbContext db, CancellationToken ct) =>
{
    var file = await db.Files.Include(x => x.Links).SingleOrDefaultAsync(x => x.Id == input.FileId, ct);
    if (file is null)
    {
        return Results.NotFound(new { Code = "File:NotFound", Message = "File was not found." });
    }

    var link = file.AddLink(input.Module, input.EntityType, input.EntityId, DateTimeOffset.UtcNow);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/files/{input.FileId}", new FileLinkDto(link.Id, link.FileId, link.Module, link.EntityType, link.EntityId, link.CreatedAt));
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Files.Upload));

app.Run();

public sealed record CreateFileDto(Guid? TenantId, string FileName, string ContentType, long Size, string StoragePath);
public sealed record CreateFileLinkDto(Guid FileId, string Module, string EntityType, string EntityId);
public sealed record FileDto(Guid Id, Guid? TenantId, string FileName, string ContentType, long Size, string StoragePath, DateTimeOffset CreatedAt);
public sealed record FileLinkDto(Guid Id, Guid FileId, string Module, string EntityType, string EntityId, DateTimeOffset CreatedAt);
