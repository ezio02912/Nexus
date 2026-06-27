using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Observability;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.Inventory.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.AddNexusObservability("inventory-service");

var connectionString = builder.Configuration.GetConnectionString("InventoryDb")
    ?? "Host=localhost;Port=5432;Database=inventory_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddNexusEfCore<InventoryDbContext>(connectionString);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus Inventory Service", Status = "Running" }));
app.MapGet("/health", async (InventoryDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

app.MapGet("/api/inventory/balances", async (InventoryDbContext db, Guid tenantId, string? search = null, CancellationToken ct = default) =>
{
    var query = db.StockBalances.Where(x => x.TenantId == tenantId);
    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.Trim().ToLowerInvariant();
        query = query.Where(x =>
            x.WarehouseCode.ToLower().Contains(term)
            || x.ProductCode.ToLower().Contains(term)
            || x.ProductName.ToLower().Contains(term));
    }

    var items = await query
        .OrderBy(x => x.WarehouseCode)
        .ThenBy(x => x.ProductCode)
        .Select(x => new StockBalanceDto(
            x.Id,
            x.TenantId,
            x.WarehouseCode,
            x.ProductCode,
            x.ProductName,
            x.OnHandQuantity,
            x.ReservedQuantity,
            x.OnHandQuantity - x.ReservedQuantity,
            x.UpdatedAt))
        .ToArrayAsync(ct);

    return Results.Ok(items);
});

app.MapGet("/api/inventory/products", async (InventoryDbContext db, Guid tenantId, string? search = null, CancellationToken ct = default) =>
{
    var query = db.Products.Where(x => x.TenantId == tenantId);
    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.Trim().ToLowerInvariant();
        query = query.Where(x =>
            x.ProductCode.ToLower().Contains(term)
            || x.ProductName.ToLower().Contains(term)
            || x.Category.ToLower().Contains(term)
            || x.Unit.ToLower().Contains(term));
    }

    var items = await query
        .OrderBy(x => x.ProductCode)
        .Select(x => new ProductDto(x.Id, x.TenantId, x.ProductCode, x.ProductName, x.Unit, x.Category, x.Price, x.TaxPercent, x.IsActive, x.UpdatedAt))
        .ToArrayAsync(ct);

    return Results.Ok(items);
});

app.MapPost("/api/inventory/products", async (UpsertProductDto input, InventoryDbContext db, CancellationToken ct) =>
{
    var now = DateTimeOffset.UtcNow;
    var productCode = StockBalance.NormalizeCode(input.ProductCode, 64);
    var product = await db.Products.SingleOrDefaultAsync(x => x.TenantId == input.TenantId && x.ProductCode == productCode, ct);
    if (product is null)
    {
        product = new InventoryProduct(Guid.NewGuid(), input.TenantId, productCode, input.ProductName, input.Unit, input.Category, input.Price, input.TaxPercent, input.IsActive, now);
        await db.Products.AddAsync(product, ct);
    }
    else
    {
        product.Update(input.ProductName, input.Unit, input.Category, input.Price, input.TaxPercent, input.IsActive, now);
    }

    await db.SaveChangesAsync(ct);
    return Results.Ok(new ProductDto(product.Id, product.TenantId, product.ProductCode, product.ProductName, product.Unit, product.Category, product.Price, product.TaxPercent, product.IsActive, product.UpdatedAt));
});

app.MapGet("/api/inventory/warehouses", async (InventoryDbContext db, Guid tenantId, string? search = null, CancellationToken ct = default) =>
{
    var query = db.Warehouses.Where(x => x.TenantId == tenantId);
    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.Trim().ToLowerInvariant();
        query = query.Where(x =>
            x.WarehouseCode.ToLower().Contains(term)
            || x.Name.ToLower().Contains(term)
            || x.Location.ToLower().Contains(term));
    }

    var items = await query
        .OrderBy(x => x.WarehouseCode)
        .Select(x => new WarehouseDto(x.Id, x.TenantId, x.WarehouseCode, x.Name, x.Location, x.IsActive, x.UpdatedAt))
        .ToArrayAsync(ct);

    return Results.Ok(items);
});

app.MapPost("/api/inventory/warehouses", async (UpsertWarehouseDto input, InventoryDbContext db, CancellationToken ct) =>
{
    var now = DateTimeOffset.UtcNow;
    var warehouseCode = StockBalance.NormalizeCode(input.WarehouseCode, 64);
    var warehouse = await db.Warehouses.SingleOrDefaultAsync(x => x.TenantId == input.TenantId && x.WarehouseCode == warehouseCode, ct);
    if (warehouse is null)
    {
        warehouse = new Warehouse(Guid.NewGuid(), input.TenantId, warehouseCode, input.Name, input.Location, input.IsActive, now);
        await db.Warehouses.AddAsync(warehouse, ct);
    }
    else
    {
        warehouse.Update(input.Name, input.Location, input.IsActive, now);
    }

    await db.SaveChangesAsync(ct);
    return Results.Ok(new WarehouseDto(warehouse.Id, warehouse.TenantId, warehouse.WarehouseCode, warehouse.Name, warehouse.Location, warehouse.IsActive, warehouse.UpdatedAt));
});

app.MapPost("/api/inventory/stock/import", async (ImportStockDto input, InventoryDbContext db, CancellationToken ct) =>
{
    var now = DateTimeOffset.UtcNow;
    await EnsureCatalogAsync(db, input, now, ct);
    var balance = await FindBalanceAsync(db, input.TenantId, input.WarehouseCode, input.ProductCode, ct);
    if (balance is null)
    {
        balance = new StockBalance(Guid.NewGuid(), input.TenantId, input.WarehouseCode, input.ProductCode, input.ProductName);
        await db.StockBalances.AddAsync(balance, ct);
    }

    balance.Import(input.Quantity, now);
    await db.StockMovements.AddAsync(new StockMovement(
        Guid.NewGuid(),
        input.TenantId,
        input.WarehouseCode,
        input.ProductCode,
        "IN",
        input.Quantity,
        input.SourceType ?? "MANUAL",
        input.SourceId ?? Guid.Empty,
        input.SourceNo ?? "Manual import",
        now), ct);

    await db.SaveChangesAsync(ct);
    return Results.Ok(new StockBalanceDto(balance.Id, balance.TenantId, balance.WarehouseCode, balance.ProductCode, balance.ProductName, balance.OnHandQuantity, balance.ReservedQuantity, balance.AvailableQuantity, balance.UpdatedAt));
});

app.MapPost("/api/inventory/reservations", async (ReserveStockDto input, InventoryDbContext db, CancellationToken ct) =>
{
    var sourceType = StockBalance.NormalizeCode(input.SourceType, 32);
    var existing = await db.StockReservations
        .Include(x => x.Lines)
        .SingleOrDefaultAsync(x => x.TenantId == input.TenantId && x.SourceType == sourceType && x.SourceId == input.SourceId, ct);

    if (existing is not null)
    {
        return Results.Ok(ToReservationDto(existing));
    }

    foreach (var line in input.Lines)
    {
        var balance = await FindBalanceAsync(db, input.TenantId, "MAIN", line.ProductCode, ct);
        if (balance is null || !balance.CanReserve(line.Quantity))
        {
            var available = balance?.AvailableQuantity ?? 0;
            return Results.Conflict(new
            {
                Code = "Inventory:InsufficientStock",
                Message = $"Không đủ tồn kho cho {line.ProductCode}. Khả dụng {available:N2}, cần {line.Quantity:N2}."
            });
        }
    }

    var now = DateTimeOffset.UtcNow;
    foreach (var line in input.Lines)
    {
        var balance = await FindBalanceAsync(db, input.TenantId, "MAIN", line.ProductCode, ct);
        balance!.Reserve(line.Quantity, now);
    }

    var reservation = new StockReservation(Guid.NewGuid(), input.TenantId, sourceType, input.SourceId, input.SourceNo, input.Lines, now);
    await db.StockReservations.AddAsync(reservation, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/inventory/reservations/{reservation.Id}", ToReservationDto(reservation));
});

app.MapPost("/api/inventory/shipments", async (ShipStockDto input, InventoryDbContext db, CancellationToken ct) =>
{
    var sourceType = StockBalance.NormalizeCode(input.SourceType, 32);
    var reservation = await db.StockReservations
        .Include(x => x.Lines)
        .SingleOrDefaultAsync(x => x.TenantId == input.TenantId && x.SourceType == sourceType && x.SourceId == input.SourceId, ct);

    if (reservation is null)
    {
        return Results.NotFound(new { Code = "Inventory:ReservationNotFound", Message = "Không tìm thấy phiếu giữ hàng cho chứng từ nguồn." });
    }

    if (reservation.Status == "Shipped")
    {
        return Results.Ok(ToReservationDto(reservation));
    }

    var now = DateTimeOffset.UtcNow;
    foreach (var line in reservation.Lines)
    {
        var balance = await FindBalanceAsync(db, input.TenantId, line.WarehouseCode, line.ProductCode, ct);
        if (balance is null)
        {
            return Results.Conflict(new { Code = "Inventory:MissingBalance", Message = $"Không tìm thấy tồn kho cho {line.ProductCode}." });
        }

        balance.ShipReserved(line.Quantity, now);
        await db.StockMovements.AddAsync(new StockMovement(
            Guid.NewGuid(),
            input.TenantId,
            line.WarehouseCode,
            line.ProductCode,
            "OUT",
            line.Quantity,
            sourceType,
            input.SourceId,
            input.SourceNo,
            now), ct);
    }

    reservation.MarkShipped(now);
    await db.SaveChangesAsync(ct);
    return Results.Ok(ToReservationDto(reservation));
});

app.MapNexusObservability();
app.Run();

static Task<StockBalance?> FindBalanceAsync(InventoryDbContext db, Guid tenantId, string warehouseCode, string productCode, CancellationToken ct)
{
    var normalizedWarehouse = StockBalance.NormalizeCode(warehouseCode, 64);
    var normalizedProduct = StockBalance.NormalizeCode(productCode, 64);
    return db.StockBalances.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.WarehouseCode == normalizedWarehouse && x.ProductCode == normalizedProduct, ct);
}

static async Task EnsureCatalogAsync(InventoryDbContext db, ImportStockDto input, DateTimeOffset now, CancellationToken ct)
{
    var productCode = StockBalance.NormalizeCode(input.ProductCode, 64);
    if (!await db.Products.AnyAsync(x => x.TenantId == input.TenantId && x.ProductCode == productCode, ct))
    {
        await db.Products.AddAsync(new InventoryProduct(
            Guid.NewGuid(),
            input.TenantId,
            productCode,
            input.ProductName,
            "EA",
            string.Empty,
            0,
            0,
            true,
            now), ct);
    }

    var warehouseCode = StockBalance.NormalizeCode(input.WarehouseCode, 64);
    if (!await db.Warehouses.AnyAsync(x => x.TenantId == input.TenantId && x.WarehouseCode == warehouseCode, ct))
    {
        await db.Warehouses.AddAsync(new Warehouse(
            Guid.NewGuid(),
            input.TenantId,
            warehouseCode,
            warehouseCode,
            string.Empty,
            true,
            now), ct);
    }
}

static StockReservationDto ToReservationDto(StockReservation reservation)
{
    return new StockReservationDto(
        reservation.Id,
        reservation.TenantId,
        reservation.SourceType,
        reservation.SourceId,
        reservation.SourceNo,
        reservation.Status,
        reservation.Lines.Select(x => new StockReservationLineDto(x.ProductCode, x.Description, x.Quantity)).ToArray(),
        reservation.CreatedAt,
        reservation.ShippedAt);
}

public sealed record StockBalanceDto(Guid Id, Guid TenantId, string WarehouseCode, string ProductCode, string ProductName, decimal OnHandQuantity, decimal ReservedQuantity, decimal AvailableQuantity, DateTimeOffset UpdatedAt);
public sealed record ImportStockDto(Guid TenantId, string WarehouseCode, string ProductCode, string ProductName, decimal Quantity, string? SourceType, Guid? SourceId, string? SourceNo);
public sealed record ProductDto(Guid Id, Guid TenantId, string ProductCode, string ProductName, string Unit, string Category, decimal Price, decimal TaxPercent, bool IsActive, DateTimeOffset UpdatedAt);
public sealed record UpsertProductDto(Guid TenantId, string ProductCode, string ProductName, string Unit, string? Category, decimal Price, decimal TaxPercent, bool IsActive);
public sealed record WarehouseDto(Guid Id, Guid TenantId, string WarehouseCode, string Name, string Location, bool IsActive, DateTimeOffset UpdatedAt);
public sealed record UpsertWarehouseDto(Guid TenantId, string WarehouseCode, string Name, string? Location, bool IsActive);
public sealed record ReserveStockDto(Guid TenantId, string SourceType, Guid SourceId, string SourceNo, IReadOnlyCollection<ReserveStockLineDto> Lines);
public sealed record ShipStockDto(Guid TenantId, string SourceType, Guid SourceId, string SourceNo);
public sealed record StockReservationDto(Guid Id, Guid TenantId, string SourceType, Guid SourceId, string SourceNo, string Status, IReadOnlyCollection<StockReservationLineDto> Lines, DateTimeOffset CreatedAt, DateTimeOffset? ShippedAt);
public sealed record StockReservationLineDto(string ProductCode, string Description, decimal Quantity);
