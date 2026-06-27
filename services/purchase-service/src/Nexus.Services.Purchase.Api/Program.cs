using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Observability;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.Purchase.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.AddNexusObservability("purchase-service");

var connectionString = builder.Configuration.GetConnectionString("PurchaseDb")
    ?? "Host=localhost;Port=5432;Database=purchase_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddHttpContextAccessor();
builder.Services.AddNexusEfCore<PurchaseDbContext>(connectionString);
builder.Services.AddHttpClient<InventoryClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CoreServices:Inventory"] ?? "http://localhost:7210");
});
builder.Services.AddHttpClient<NumberingClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Numbering"] ?? "http://localhost:7206");
});

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus Purchase Service", Status = "Running" }));
app.MapGet("/health", async (PurchaseDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

app.MapGet("/api/purchase/suppliers", async (PurchaseDbContext db, Guid tenantId, string? search = null, CancellationToken ct = default) =>
{
    var query = db.Suppliers.Where(x => x.TenantId == tenantId);
    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.Trim().ToLowerInvariant();
        query = query.Where(x =>
            x.SupplierCode.ToLower().Contains(term)
            || x.Name.ToLower().Contains(term)
            || x.Email.ToLower().Contains(term)
            || x.Phone.ToLower().Contains(term));
    }

    var items = await query.OrderBy(x => x.SupplierCode).ToArrayAsync(ct);
    return Results.Ok(items);
});

app.MapPost("/api/purchase/suppliers", async (UpsertSupplierDto input, PurchaseDbContext db, CancellationToken ct) =>
{
    var now = DateTimeOffset.UtcNow;
    var supplierCode = Supplier.NormalizeCode(input.SupplierCode, 64);
    var supplier = await db.Suppliers.SingleOrDefaultAsync(x => x.TenantId == input.TenantId && x.SupplierCode == supplierCode, ct);
    if (supplier is null)
    {
        supplier = new Supplier(Guid.NewGuid(), input.TenantId, supplierCode, input.Name, input.Email, input.Phone, now);
        await db.Suppliers.AddAsync(supplier, ct);
    }
    else
    {
        supplier.Update(input.Name, input.Email, input.Phone, now);
    }

    await db.SaveChangesAsync(ct);
    return Results.Ok(supplier);
});

app.MapGet("/api/purchase/orders", async (PurchaseDbContext db, Guid tenantId, string? search = null, CancellationToken ct = default) =>
{
    var query = db.PurchaseOrders
        .Include(x => x.Lines)
        .Where(x => x.TenantId == tenantId);
    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.Trim().ToLowerInvariant();
        query = query.Where(x =>
            x.PurchaseOrderNo.ToLower().Contains(term)
            || x.SupplierCode.ToLower().Contains(term)
            || x.SupplierName.ToLower().Contains(term)
            || x.Status.ToLower().Contains(term)
            || x.Lines.Any(line =>
                line.ProductCode.ToLower().Contains(term)
                || line.ProductName.ToLower().Contains(term)));
    }

    var items = await query.OrderByDescending(x => x.CreatedAt).ToArrayAsync(ct);
    return Results.Ok(items);
});

app.MapPost("/api/purchase/orders", async (CreatePurchaseOrderDto input, PurchaseDbContext db, NumberingClient numbering, CancellationToken ct) =>
{
    var rawNo = (input.PurchaseOrderNo ?? string.Empty).Trim();
    var purchaseOrderNo = string.IsNullOrWhiteSpace(rawNo) || rawNo.Equals("AUTO", StringComparison.OrdinalIgnoreCase)
        ? await numbering.GetNextNumberAsync(input.TenantId, "PURCHASE", "ORDER", "PO-", ct)
        : Supplier.NormalizeCode(rawNo, 64);
    if (await db.PurchaseOrders.AnyAsync(x => x.TenantId == input.TenantId && x.PurchaseOrderNo == purchaseOrderNo, ct))
    {
        return Results.Conflict(new { Code = "Purchase:OrderAlreadyExists", Message = "Purchase order number already exists." });
    }

    var supplierCode = Supplier.NormalizeCode(input.SupplierCode, 64);
    var supplier = await db.Suppliers.SingleOrDefaultAsync(x => x.TenantId == input.TenantId && x.SupplierCode == supplierCode, ct);
    if (supplier is null)
    {
        return Results.BadRequest(new { Code = "Purchase:SupplierNotFound", Message = "Không tìm thấy nhà cung cấp." });
    }

    var lines = input.Lines
        .Select(x => new PurchaseOrderLineDraft(x.WarehouseCode, x.ProductCode, x.ProductName, x.Quantity, x.UnitCost))
        .ToArray();
    var order = new PurchaseOrder(Guid.NewGuid(), input.TenantId, purchaseOrderNo, supplier.SupplierCode, supplier.Name, lines, DateTimeOffset.UtcNow);
    await db.PurchaseOrders.AddAsync(order, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/purchase/orders/{order.Id}", order);
});

app.MapPost("/api/purchase/orders/{id:guid}/approve", async (Guid id, PurchaseDbContext db, CancellationToken ct) =>
{
    var order = await db.PurchaseOrders.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id, ct);
    if (order is null)
    {
        return Results.NotFound();
    }

    if (order.Status == "Draft")
    {
        order.Approve(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(ct);
    }

    return Results.Ok(order);
});

app.MapPost("/api/purchase/orders/{id:guid}/unapprove", async (Guid id, PurchaseDbContext db, CancellationToken ct) =>
{
    var order = await db.PurchaseOrders.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id, ct);
    if (order is null)
    {
        return Results.NotFound();
    }

    if (!order.CanUnapprove)
    {
        return Results.Conflict(new { Code = "Purchase:CannotUnapprove", Message = "Chỉ huỷ duyệt được đơn đã duyệt và chưa nhận hàng." });
    }

    order.Unapprove();
    await db.SaveChangesAsync(ct);
    return Results.Ok(order);
});

app.MapDelete("/api/purchase/orders/{id:guid}", async (Guid id, PurchaseDbContext db, CancellationToken ct) =>
{
    var order = await db.PurchaseOrders.SingleOrDefaultAsync(x => x.Id == id, ct);
    if (order is null)
    {
        return Results.NotFound();
    }

    if (!order.CanDelete)
    {
        return Results.Conflict(new { Code = "Purchase:CannotDelete", Message = "Chỉ xoá được đơn ở trạng thái nháp." });
    }

    db.PurchaseOrders.Remove(order);
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

app.MapPost("/api/purchase/orders/{id:guid}/receive", async (Guid id, ReceivePurchaseOrderDto input, PurchaseDbContext db, InventoryClient inventory, NumberingClient numbering, CancellationToken ct) =>
{
    var order = await db.PurchaseOrders.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id, ct);
    if (order is null)
    {
        return Results.NotFound();
    }

    if (order.Status == "Received")
    {
        return Results.Ok(order);
    }

    if (order.Status == "Draft")
    {
        return Results.Conflict(new { Code = "Purchase:OrderNotApproved", Message = "Đơn mua hàng chưa duyệt, không thể nhận hàng." });
    }

    var receiptNo = string.IsNullOrWhiteSpace(input.ReceiptNo)
        ? await numbering.GetNextNumberAsync(order.TenantId, "PURCHASE", "RECEIPT", "GR-", ct)
        : Supplier.NormalizeCode(input.ReceiptNo, 64);
    if (await db.GoodsReceipts.AnyAsync(x => x.TenantId == order.TenantId && x.ReceiptNo == receiptNo, ct))
    {
        return Results.Conflict(new { Code = "Purchase:ReceiptAlreadyExists", Message = "Goods receipt number already exists." });
    }

    var receipt = new GoodsReceipt(Guid.NewGuid(), order.TenantId, order.Id, order.PurchaseOrderNo, receiptNo, order.Lines, DateTimeOffset.UtcNow);
    foreach (var line in receipt.Lines)
    {
        var result = await inventory.ImportAsync(new InventoryImportRequest(
            order.TenantId,
            line.WarehouseCode,
            line.ProductCode,
            line.ProductName,
            line.Quantity,
            "PURCHASE_RECEIPT",
            receipt.Id,
            receipt.ReceiptNo), ct);

        if (!result.Success)
        {
            return Results.Conflict(new { Code = result.Code, Message = result.Message });
        }
    }

    order.MarkReceived(receipt.ReceivedAt);
    await db.GoodsReceipts.AddAsync(receipt, ct);
    await db.SaveChangesAsync(ct);
    return Results.Ok(order);
});

app.MapGet("/api/purchase/goods-receipts", async (PurchaseDbContext db, Guid tenantId, CancellationToken ct = default) =>
{
    var items = await db.GoodsReceipts
        .Include(x => x.Lines)
        .Where(x => x.TenantId == tenantId)
        .OrderByDescending(x => x.ReceivedAt)
        .ToArrayAsync(ct);
    return Results.Ok(items);
});

app.MapNexusObservability();
app.Run();

public sealed record UpsertSupplierDto(Guid TenantId, string SupplierCode, string Name, string? Email, string? Phone);
public sealed record CreatePurchaseOrderDto(Guid TenantId, string PurchaseOrderNo, string SupplierCode, IReadOnlyCollection<CreatePurchaseOrderLineDto> Lines);
public sealed record CreatePurchaseOrderLineDto(string WarehouseCode, string ProductCode, string ProductName, decimal Quantity, decimal UnitCost);
public sealed record ReceivePurchaseOrderDto(string? ReceiptNo);

public sealed class InventoryClient
{
    private readonly HttpClient _httpClient;

    public InventoryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<InventoryCallResult> ImportAsync(InventoryImportRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/inventory/stock/import", request, ct);
            if (response.IsSuccessStatusCode)
            {
                return InventoryCallResult.Ok();
            }

            var error = await response.Content.ReadFromJsonAsync<InventoryErrorResponse>(cancellationToken: ct);
            return InventoryCallResult.Failed(
                string.IsNullOrWhiteSpace(error?.Code) ? "Inventory:ImportFailed" : error!.Code!,
                string.IsNullOrWhiteSpace(error?.Message) ? "Inventory import failed." : error!.Message!);
        }
        catch (HttpRequestException ex)
        {
            return InventoryCallResult.Failed("Inventory:Unavailable", ex.Message);
        }
        catch (JsonException ex)
        {
            return InventoryCallResult.Failed("Inventory:InvalidErrorResponse", ex.Message);
        }
    }
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

public sealed record InventoryImportRequest(Guid TenantId, string WarehouseCode, string ProductCode, string ProductName, decimal Quantity, string SourceType, Guid SourceId, string SourceNo);
public sealed record InventoryErrorResponse(string? Code, string? Message);
public sealed record InventoryCallResult(bool Success, string Code, string Message)
{
    public static InventoryCallResult Ok() => new(true, string.Empty, string.Empty);
    public static InventoryCallResult Failed(string code, string message) => new(false, code, message);
}
