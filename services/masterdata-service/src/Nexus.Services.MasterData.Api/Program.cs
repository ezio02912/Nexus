using Nexus.BuildingBlocks.Observability;
using Microsoft.EntityFrameworkCore;
using Nexus.ApiContracts.Permissions;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Web.Auth;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.MasterData.Api.Persistence;
using MasterDataCategories = Nexus.Services.MasterData.Api.MasterDataCategories;

var builder = WebApplication.CreateBuilder(args);
builder.AddNexusObservability("masterdata-service");

var connectionString = builder.Configuration.GetConnectionString("MasterDataDb")
    ?? "Host=localhost;Port=5432;Database=masterdata_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddNexusJwtAuth(builder.Configuration);
builder.Services.AddNexusEfCore<MasterDataDbContext>(connectionString);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MasterDataDbContext>();
    await db.Database.MigrateAsync();
    await SeedLookupItemsAsync(db);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus Master Data Service", Status = "Running" }));
app.MapGet("/health", async (MasterDataDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

var masterData = app.MapGroup("/api/master-data").RequireAuthorization();

masterData.MapGet("/categories", () =>
    Results.Ok(MasterDataCategories.All.Select(x => new MasterDataCategoryDto(x.Code, x.Label))));

masterData.MapGet("/items", async (
    string category,
    string? search,
    int skipCount,
    int maxResultCount,
    MasterDataDbContext db,
    CancellationToken ct) =>
{
    if (!MasterDataCategories.IsValid(category))
    {
        return Results.BadRequest(new { Message = "Danh mục không hợp lệ." });
    }

    var query = db.LookupItems.AsNoTracking()
        .Where(x => x.Category == category && x.IsActive);

    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.Trim().ToLowerInvariant();
        query = query.Where(x => x.Name.ToLower().Contains(term) || x.Code.ToLower().Contains(term));
    }

    var total = await query.LongCountAsync(ct);
    var items = await query
        .OrderBy(x => x.SortOrder)
        .ThenBy(x => x.Name)
        .Skip(Math.Max(skipCount, 0))
        .Take(Math.Clamp(maxResultCount <= 0 ? 100 : maxResultCount, 1, 500))
        .Select(x => new LookupItemDto(x.Id, x.Category, x.Code, x.Name, x.SortOrder, x.IsActive))
        .ToArrayAsync(ct);

    return Results.Ok(new PagedLookupItemsDto(total, items));
});

masterData.MapGet("/admin/items", async (string? category, MasterDataDbContext db, CancellationToken ct) =>
{
    var query = db.LookupItems.AsNoTracking();
    if (!string.IsNullOrWhiteSpace(category))
    {
        query = query.Where(x => x.Category == category);
    }

    var items = await query
        .OrderBy(x => x.Category)
        .ThenBy(x => x.SortOrder)
        .ThenBy(x => x.Name)
        .Select(x => new LookupItemDto(x.Id, x.Category, x.Code, x.Name, x.SortOrder, x.IsActive))
        .ToArrayAsync(ct);

    return Results.Ok(items);
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.MasterData.Manage));

masterData.MapPost("/admin/items", async (CreateLookupItemDto input, MasterDataDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(input.Category) || string.IsNullOrWhiteSpace(input.Code) || string.IsNullOrWhiteSpace(input.Name))
    {
        return Results.BadRequest(new { Message = "Vui lòng nhập danh mục, mã và tên hiển thị." });
    }

    if (!MasterDataCategories.IsValid(input.Category))
    {
        return Results.BadRequest(new { Message = $"Danh mục '{input.Category}' không hợp lệ. Hãy restart MasterData service nếu vừa thêm danh mục mới." });
    }

    var code = input.Code.Trim().ToUpperInvariant();
    if (await db.LookupItems.AnyAsync(x => x.Category == input.Category && x.Code == code, ct))
    {
        return Results.Conflict(new { Message = "Mã đã tồn tại trong danh mục này." });
    }

    var item = new LookupItem(Guid.NewGuid(), input.Category, code, input.Name.Trim(), input.SortOrder, input.IsActive);
    await db.LookupItems.AddAsync(item, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/master-data/admin/items/{item.Id}",
        new LookupItemDto(item.Id, item.Category, item.Code, item.Name, item.SortOrder, item.IsActive));
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.MasterData.Manage));

masterData.MapPut("/admin/items/{id:guid}", async (Guid id, UpdateLookupItemDto input, MasterDataDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(input.Code) || string.IsNullOrWhiteSpace(input.Name))
    {
        return Results.BadRequest(new { Message = "Vui lòng nhập mã và tên hiển thị." });
    }

    var item = await db.LookupItems.FirstOrDefaultAsync(x => x.Id == id, ct);
    if (item is null)
    {
        return Results.NotFound();
    }

    var code = input.Code.Trim().ToUpperInvariant();
    if (await db.LookupItems.AnyAsync(x => x.Id != id && x.Category == item.Category && x.Code == code, ct))
    {
        return Results.Conflict(new { Message = "Mã đã tồn tại trong danh mục này." });
    }

    item.Update(code, input.Name, input.SortOrder, input.IsActive);
    await db.SaveChangesAsync(ct);
    return Results.Ok(new LookupItemDto(item.Id, item.Category, item.Code, item.Name, item.SortOrder, item.IsActive));
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.MasterData.Manage));

masterData.MapDelete("/admin/items/{id:guid}", async (Guid id, MasterDataDbContext db, CancellationToken ct) =>
{
    var item = await db.LookupItems.FirstOrDefaultAsync(x => x.Id == id, ct);
    if (item is null)
    {
        return Results.NotFound();
    }

    db.LookupItems.Remove(item);
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
}).RequireAuthorization(NexusPolicies.Permission(NexusPermissions.MasterData.Manage));

app.MapNexusObservability();
app.Run();

static async Task SeedLookupItemsAsync(MasterDataDbContext db)
{
    var seeds = new List<LookupItem>
    {
        new(Guid.NewGuid(), MasterDataCategories.Industry, "IT", "Công nghệ thông tin", 1, true),
        new(Guid.NewGuid(), MasterDataCategories.Industry, "FIN", "Tài chính - Ngân hàng", 2, true),
        new(Guid.NewGuid(), MasterDataCategories.Industry, "MFG", "Sản xuất", 3, true),
        new(Guid.NewGuid(), MasterDataCategories.Industry, "RET", "Bán lẻ", 4, true),
        new(Guid.NewGuid(), MasterDataCategories.City, "HCM", "TP. Hồ Chí Minh", 1, true),
        new(Guid.NewGuid(), MasterDataCategories.City, "HN", "Hà Nội", 2, true),
        new(Guid.NewGuid(), MasterDataCategories.City, "DN", "Đà Nẵng", 3, true),
        new(Guid.NewGuid(), MasterDataCategories.City, "HP", "Hải Phòng", 4, true),
        new(Guid.NewGuid(), MasterDataCategories.Source, "WEB", "Website", 1, true),
        new(Guid.NewGuid(), MasterDataCategories.Source, "REF", "Giới thiệu", 2, true),
        new(Guid.NewGuid(), MasterDataCategories.Source, "ADS", "Quảng cáo", 3, true),
        new(Guid.NewGuid(), MasterDataCategories.Source, "EVT", "Sự kiện", 4, true),
        new(Guid.NewGuid(), MasterDataCategories.Unit, "EA", "Cái", 1, true),
        new(Guid.NewGuid(), MasterDataCategories.Unit, "BOX", "Hộp", 2, true),
        new(Guid.NewGuid(), MasterDataCategories.Unit, "CTN", "Thùng", 3, true),
        new(Guid.NewGuid(), MasterDataCategories.Unit, "SET", "Bộ", 4, true),
        new(Guid.NewGuid(), MasterDataCategories.Unit, "KG", "Kilogram", 5, true),
        new(Guid.NewGuid(), MasterDataCategories.Unit, "G", "Gram", 6, true),
        new(Guid.NewGuid(), MasterDataCategories.Unit, "L", "Lít", 7, true),
        new(Guid.NewGuid(), MasterDataCategories.Unit, "M", "Mét", 8, true),
        new(Guid.NewGuid(), MasterDataCategories.Unit, "M2", "Mét vuông", 9, true),
        new(Guid.NewGuid(), MasterDataCategories.Unit, "PAIR", "Đôi", 10, true),
        new(Guid.NewGuid(), MasterDataCategories.ProductType, "GOODS", "Hàng hoá", 1, true),
        new(Guid.NewGuid(), MasterDataCategories.ProductType, "SERVICE", "Dịch vụ", 2, true),
        new(Guid.NewGuid(), MasterDataCategories.ProductType, "COMBO", "Combo/Bộ sản phẩm", 3, true),
        new(Guid.NewGuid(), MasterDataCategories.ProductType, "MATERIAL", "Nguyên vật liệu", 4, true),
        new(Guid.NewGuid(), MasterDataCategories.ProductType, "ASSET", "Tài sản", 5, true),
    };

    // Idempotent seed: only insert items whose (Category, Code) is not present yet,
    // so new categories (e.g. Unit) are added even when the table already has data.
    var existing = await db.LookupItems
        .Select(x => new { x.Category, x.Code })
        .ToListAsync();
    var existingKeys = existing
        .Select(x => $"{x.Category}|{x.Code}")
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    var missing = seeds
        .Where(x => existingKeys.Add($"{x.Category}|{x.Code}"))
        .ToList();

    if (missing.Count == 0)
    {
        return;
    }

    await db.LookupItems.AddRangeAsync(missing);
    await db.SaveChangesAsync();
}

public sealed record MasterDataCategoryDto(string Code, string Label);
public sealed record LookupItemDto(Guid Id, string Category, string Code, string Name, int SortOrder, bool IsActive);
public sealed record PagedLookupItemsDto(long TotalCount, IReadOnlyList<LookupItemDto> Items);
public sealed record CreateLookupItemDto(string Category, string Code, string Name, int SortOrder, bool IsActive);
public sealed record UpdateLookupItemDto(string Code, string Name, int SortOrder, bool IsActive);
