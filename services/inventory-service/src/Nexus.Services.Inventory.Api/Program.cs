using System.Net.Http.Json;
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
builder.Services.AddHttpContextAccessor();
builder.Services.AddNexusEfCore<InventoryDbContext>(connectionString);
builder.Services.AddHttpClient<NumberingClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Numbering"] ?? "http://localhost:7206");
});

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
            || x.Unit.ToLower().Contains(term)
            || x.Attributes.ToLower().Contains(term)
            || x.Variants.ToLower().Contains(term));
    }

    var items = await query
        .OrderBy(x => x.ProductCode)
        .Select(x => new ProductDto(x.Id, x.TenantId, x.ProductCode, x.ProductName, x.Unit, x.Category, x.Price, x.TaxPercent, x.IsActive, x.Attributes, x.Variants, x.UpdatedAt))
        .ToArrayAsync(ct);

    return Results.Ok(items);
});

app.MapPost("/api/inventory/products", async (UpsertProductDto input, InventoryDbContext db, NumberingClient numbering, CancellationToken ct) =>
{
    var now = DateTimeOffset.UtcNow;
    var rawCode = (input.ProductCode ?? string.Empty).Trim();
    var productCode = string.IsNullOrWhiteSpace(rawCode) || rawCode.Equals("AUTO", StringComparison.OrdinalIgnoreCase)
        ? await numbering.GetNextNumberAsync(input.TenantId, "INVENTORY", "PRODUCT", "SKU-", ct)
        : StockBalance.NormalizeCode(rawCode, 64);
    var product = await db.Products.SingleOrDefaultAsync(x => x.TenantId == input.TenantId && x.ProductCode == productCode, ct);
    if (product is null)
    {
        product = new InventoryProduct(Guid.NewGuid(), input.TenantId, productCode, input.ProductName, input.Unit, input.Category, input.Price, input.TaxPercent, input.IsActive, input.Attributes, input.Variants, now);
        await db.Products.AddAsync(product, ct);
    }
    else
    {
        product.Update(input.ProductName, input.Unit, input.Category, input.Price, input.TaxPercent, input.IsActive, input.Attributes, input.Variants, now);
    }

    await db.SaveChangesAsync(ct);
    return Results.Ok(ToProductDto(product));
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
        .Select(x => new WarehouseDto(x.Id, x.TenantId, x.WarehouseCode, x.Name, x.Location, x.IsActive, x.AllowNegativeStock, x.UpdatedAt))
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
        warehouse = new Warehouse(Guid.NewGuid(), input.TenantId, warehouseCode, input.Name, input.Location, input.IsActive, input.AllowNegativeStock, now);
        await db.Warehouses.AddAsync(warehouse, ct);
    }
    else
    {
        warehouse.Update(input.Name, input.Location, input.IsActive, input.AllowNegativeStock, now);
    }

    await db.SaveChangesAsync(ct);
    return Results.Ok(new WarehouseDto(warehouse.Id, warehouse.TenantId, warehouse.WarehouseCode, warehouse.Name, warehouse.Location, warehouse.IsActive, warehouse.AllowNegativeStock, warehouse.UpdatedAt));
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

app.MapGet("/api/inventory/movements", async (InventoryDbContext db, Guid tenantId, string? productCode = null, string? warehouseCode = null, DateTimeOffset? from = null, DateTimeOffset? to = null, int maxResultCount = 200, CancellationToken ct = default) =>
{
    var query = db.StockMovements.Where(x => x.TenantId == tenantId);
    if (!string.IsNullOrWhiteSpace(productCode))
    {
        var normalizedProduct = StockBalance.NormalizeCode(productCode, 64);
        query = query.Where(x => x.ProductCode == normalizedProduct);
    }

    if (!string.IsNullOrWhiteSpace(warehouseCode))
    {
        var normalizedWarehouse = StockBalance.NormalizeCode(warehouseCode, 64);
        query = query.Where(x => x.WarehouseCode == normalizedWarehouse);
    }

    if (from.HasValue)
    {
        query = query.Where(x => x.OccurredAt >= from.Value);
    }

    if (to.HasValue)
    {
        query = query.Where(x => x.OccurredAt <= to.Value);
    }

    var items = await query
        .OrderByDescending(x => x.OccurredAt)
        .Take(Math.Clamp(maxResultCount, 1, 1000))
        .Select(x => new StockMovementDto(
            x.Id,
            x.TenantId,
            x.WarehouseCode,
            x.ProductCode,
            x.MovementType,
            x.Quantity,
            x.SourceType,
            x.SourceId,
            x.SourceNo,
            x.OccurredAt))
        .ToArrayAsync(ct);

    return Results.Ok(items);
});

app.MapGet("/api/inventory/transfers", async (InventoryDbContext db, Guid tenantId, string? search = null, CancellationToken ct = default) =>
{
    var query = db.StockTransfers.Where(x => x.TenantId == tenantId);
    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.Trim().ToLowerInvariant();
        query = query.Where(x =>
            x.TransferNo.ToLower().Contains(term)
            || x.ProductCode.ToLower().Contains(term)
            || x.ProductName.ToLower().Contains(term)
            || x.FromWarehouseCode.ToLower().Contains(term)
            || x.ToWarehouseCode.ToLower().Contains(term));
    }

    var rows = await query
        .OrderByDescending(x => x.CreatedAt)
        .ToArrayAsync(ct);

    // Map after materialization; ToTransferDto is a local function and cannot live in an EF expression tree.
    var items = rows.Select(ToTransferDto).ToArray();

    return Results.Ok(items);
});

app.MapGet("/api/inventory/transfers/{id:guid}", async (Guid id, InventoryDbContext db, Guid tenantId, CancellationToken ct) =>
{
    var transfer = await db.StockTransfers.SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
    return transfer is null ? Results.NotFound() : Results.Ok(ToTransferDto(transfer));
});

app.MapPost("/api/inventory/transfers", async (TransferStockDto input, InventoryDbContext db, NumberingClient numbering, CancellationToken ct) =>
{
    var fromWarehouse = StockBalance.NormalizeCode(string.IsNullOrWhiteSpace(input.FromWarehouseCode) ? "MAIN" : input.FromWarehouseCode, 64);
    var toWarehouse = StockBalance.NormalizeCode(string.IsNullOrWhiteSpace(input.ToWarehouseCode) ? "MAIN" : input.ToWarehouseCode, 64);
    var productCode = StockBalance.NormalizeCode(input.ProductCode, 64);

    if (input.Quantity <= 0)
    {
        return Results.Conflict(new { Code = "Inventory:InvalidQuantity", Message = "Số lượng chuyển phải lớn hơn 0." });
    }

    if (fromWarehouse == toWarehouse)
    {
        return Results.Conflict(new { Code = "Inventory:SameWarehouse", Message = "Kho nguồn và kho đích phải khác nhau." });
    }

    var allowNegativeStock = await WarehouseAllowsNegativeStockAsync(db, input.TenantId, fromWarehouse, ct);
    var source = await FindBalanceAsync(db, input.TenantId, fromWarehouse, productCode, ct);
    if ((source is null || source.AvailableQuantity < input.Quantity) && !allowNegativeStock)
    {
        var available = source?.AvailableQuantity ?? 0;
        return Results.Conflict(new
        {
            Code = "Inventory:InsufficientStock",
            Message = $"Không đủ tồn kho tại {fromWarehouse}/{productCode}. Khả dụng {available:N2}, cần {input.Quantity:N2}."
        });
    }

    var now = DateTimeOffset.UtcNow;
    var productName = string.IsNullOrWhiteSpace(input.ProductName) ? source?.ProductName ?? productCode : input.ProductName;
    var transferNo = await numbering.GetNextNumberAsync(input.TenantId, "INVENTORY", "TRANSFER", "TR-", ct);

    var transfer = new StockTransfer(Guid.NewGuid(), input.TenantId, transferNo, fromWarehouse, toWarehouse, productCode, productName, input.Quantity, now);
    await db.StockTransfers.AddAsync(transfer, ct);

    if (source is null)
    {
        source = new StockBalance(Guid.NewGuid(), input.TenantId, fromWarehouse, productCode, productName);
        await db.StockBalances.AddAsync(source, ct);
    }

    source.RemoveOnHand(input.Quantity, now, allowNegativeStock);

    var destination = await FindBalanceAsync(db, input.TenantId, toWarehouse, productCode, ct);
    if (destination is null)
    {
        destination = new StockBalance(Guid.NewGuid(), input.TenantId, toWarehouse, productCode, productName);
        await db.StockBalances.AddAsync(destination, ct);
    }

    destination.Import(input.Quantity, now);

    // Link both movements to the transfer document so the stock detail view can navigate back to it.
    await db.StockMovements.AddAsync(new StockMovement(
        Guid.NewGuid(), input.TenantId, fromWarehouse, productCode, "TRANSFER_OUT", input.Quantity, "TRANSFER", transfer.Id, transferNo, now), ct);
    await db.StockMovements.AddAsync(new StockMovement(
        Guid.NewGuid(), input.TenantId, toWarehouse, productCode, "TRANSFER_IN", input.Quantity, "TRANSFER", transfer.Id, transferNo, now), ct);

    await db.SaveChangesAsync(ct);

    return Results.Created($"/api/inventory/transfers/{transfer.Id}", ToTransferDto(transfer));
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
        var balance = await FindBalanceAsync(db, input.TenantId, line.WarehouseCode, line.ProductCode, ct);
        var allowNegativeStock = await WarehouseAllowsNegativeStockAsync(db, input.TenantId, line.WarehouseCode, ct);
        if ((balance is null || !balance.CanReserve(line.Quantity, allowNegativeStock)) && !allowNegativeStock)
        {
            var available = balance?.AvailableQuantity ?? 0;
            return Results.Conflict(new
            {
                Code = "Inventory:InsufficientStock",
                Message = $"Không đủ tồn kho cho {line.WarehouseCode}/{line.ProductCode}. Khả dụng {available:N2}, cần {line.Quantity:N2}."
            });
        }
    }

    var now = DateTimeOffset.UtcNow;
    foreach (var line in input.Lines)
    {
        var balance = await FindBalanceAsync(db, input.TenantId, line.WarehouseCode, line.ProductCode, ct);
        var allowNegativeStock = await WarehouseAllowsNegativeStockAsync(db, input.TenantId, line.WarehouseCode, ct);
        if (balance is null)
        {
            balance = new StockBalance(Guid.NewGuid(), input.TenantId, line.WarehouseCode, line.ProductCode, line.Description);
            await db.StockBalances.AddAsync(balance, ct);
        }

        balance.Reserve(line.Quantity, now, allowNegativeStock);
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

app.MapPost("/api/inventory/reservations/release", async (ShipStockDto input, InventoryDbContext db, CancellationToken ct) =>
{
    var sourceType = StockBalance.NormalizeCode(input.SourceType, 32);
    var reservation = await db.StockReservations
        .Include(x => x.Lines)
        .SingleOrDefaultAsync(x => x.TenantId == input.TenantId && x.SourceType == sourceType && x.SourceId == input.SourceId, ct);

    if (reservation is null)
    {
        // Nothing reserved (idempotent): treat as success so callers can safely un-approve.
        return Results.Ok(new { Released = false });
    }

    if (reservation.Status == "Shipped")
    {
        return Results.Conflict(new { Code = "Inventory:AlreadyShipped", Message = "Đã xuất kho, không thể giải phóng giữ hàng." });
    }

    var now = DateTimeOffset.UtcNow;
    foreach (var line in reservation.Lines)
    {
        var balance = await FindBalanceAsync(db, input.TenantId, line.WarehouseCode, line.ProductCode, ct);
        balance?.ReleaseReserved(line.Quantity, now);
    }

    // Remove the reservation so the source document can be re-approved later without a unique-key clash.
    db.StockReservations.Remove(reservation);
    await db.SaveChangesAsync(ct);
    return Results.Ok(new { Released = true });
});

app.MapNexusObservability();
app.Run();

static StockTransferDto ToTransferDto(StockTransfer x) => new(
    x.Id, x.TenantId, x.TransferNo, x.FromWarehouseCode, x.ToWarehouseCode, x.ProductCode, x.ProductName, x.Quantity, x.Status, x.CreatedAt);

static Task<StockBalance?> FindBalanceAsync(InventoryDbContext db, Guid tenantId, string warehouseCode, string productCode, CancellationToken ct)
{
    var normalizedWarehouse = StockBalance.NormalizeCode(string.IsNullOrWhiteSpace(warehouseCode) ? "MAIN" : warehouseCode, 64);
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
            string.Empty,
            string.Empty,
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
            false,
            now), ct);
    }
}

static async Task<bool> WarehouseAllowsNegativeStockAsync(InventoryDbContext db, Guid tenantId, string warehouseCode, CancellationToken ct)
{
    var normalizedWarehouse = StockBalance.NormalizeCode(string.IsNullOrWhiteSpace(warehouseCode) ? "MAIN" : warehouseCode, 64);
    return await db.Warehouses
        .Where(x => x.TenantId == tenantId && x.WarehouseCode == normalizedWarehouse)
        .Select(x => x.AllowNegativeStock)
        .SingleOrDefaultAsync(ct);
}

static ProductDto ToProductDto(InventoryProduct product) =>
    new(
        product.Id,
        product.TenantId,
        product.ProductCode,
        product.ProductName,
        product.Unit,
        product.Category,
        product.Price,
        product.TaxPercent,
        product.IsActive,
        product.Attributes,
        product.Variants,
        product.UpdatedAt);

static StockReservationDto ToReservationDto(StockReservation reservation)
{
    return new StockReservationDto(
        reservation.Id,
        reservation.TenantId,
        reservation.SourceType,
        reservation.SourceId,
        reservation.SourceNo,
        reservation.Status,
        reservation.Lines.Select(x => new StockReservationLineDto(x.WarehouseCode, x.ProductCode, x.Description, x.Quantity)).ToArray(),
        reservation.CreatedAt,
        reservation.ShippedAt);
}

public sealed class NumberingClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NumberingClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    // Allocates a guaranteed-unique number from the numbering-service, forwarding the caller's bearer token.
    public async Task<string> GetNextNumberAsync(Guid tenantId, string module, string documentType, string prefix, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/numbering/next")
            {
                Content = JsonContent.Create(new { TenantId = tenantId, Module = module, DocumentType = documentType, Prefix = prefix, Padding = 5 })
            };

            var authorization = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authorization))
            {
                request.Headers.TryAddWithoutValidation("Authorization", authorization);
            }

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                return $"{prefix}{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
            }

            var result = await response.Content.ReadFromJsonAsync<NextNumberResponse>(cancellationToken: ct);
            return result?.Number ?? $"{prefix}{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        }
        catch
        {
            return $"{prefix}{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        }
    }

    private sealed record NextNumberResponse(string SequenceKey, string Number, long Value);
}

public sealed record StockBalanceDto(Guid Id, Guid TenantId, string WarehouseCode, string ProductCode, string ProductName, decimal OnHandQuantity, decimal ReservedQuantity, decimal AvailableQuantity, DateTimeOffset UpdatedAt);
public sealed record StockMovementDto(Guid Id, Guid TenantId, string WarehouseCode, string ProductCode, string MovementType, decimal Quantity, string SourceType, Guid SourceId, string SourceNo, DateTimeOffset OccurredAt);
public sealed record TransferStockDto(Guid TenantId, string FromWarehouseCode, string ToWarehouseCode, string ProductCode, string? ProductName, decimal Quantity, string? SourceNo);
public sealed record StockTransferDto(Guid Id, Guid TenantId, string TransferNo, string FromWarehouseCode, string ToWarehouseCode, string ProductCode, string ProductName, decimal Quantity, string Status, DateTimeOffset CreatedAt);
public sealed record ImportStockDto(Guid TenantId, string WarehouseCode, string ProductCode, string ProductName, decimal Quantity, string? SourceType, Guid? SourceId, string? SourceNo);
public sealed record ProductDto(Guid Id, Guid TenantId, string ProductCode, string ProductName, string Unit, string Category, decimal Price, decimal TaxPercent, bool IsActive, string Attributes, string Variants, DateTimeOffset UpdatedAt);
public sealed record UpsertProductDto(Guid TenantId, string ProductCode, string ProductName, string Unit, string? Category, decimal Price, decimal TaxPercent, bool IsActive, string? Attributes, string? Variants);
public sealed record WarehouseDto(Guid Id, Guid TenantId, string WarehouseCode, string Name, string Location, bool IsActive, bool AllowNegativeStock, DateTimeOffset UpdatedAt);
public sealed record UpsertWarehouseDto(Guid TenantId, string WarehouseCode, string Name, string? Location, bool IsActive, bool AllowNegativeStock);
public sealed record ReserveStockDto(Guid TenantId, string SourceType, Guid SourceId, string SourceNo, IReadOnlyCollection<ReserveStockLineDto> Lines);
public sealed record ShipStockDto(Guid TenantId, string SourceType, Guid SourceId, string SourceNo);
public sealed record StockReservationDto(Guid Id, Guid TenantId, string SourceType, Guid SourceId, string SourceNo, string Status, IReadOnlyCollection<StockReservationLineDto> Lines, DateTimeOffset CreatedAt, DateTimeOffset? ShippedAt);
public sealed record StockReservationLineDto(string WarehouseCode, string ProductCode, string Description, decimal Quantity);
