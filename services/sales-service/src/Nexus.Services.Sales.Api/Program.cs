using Nexus.BuildingBlocks.Observability;
using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.Sales.Api.Persistence;
using System.Net.Http.Json;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.AddNexusObservability("sales-service");

var connectionString = builder.Configuration.GetConnectionString("SalesDb")
    ?? "Host=localhost;Port=5432;Database=sales_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddHttpContextAccessor();
builder.Services.AddNexusEfCore<SalesDbContext>(connectionString);
builder.Services.AddHttpClient<InventoryClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CoreServices:Inventory"] ?? "http://localhost:7210");
});
builder.Services.AddHttpClient<NumberingClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Numbering"] ?? "http://localhost:7206");
});

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus Sales Service", Status = "Running" }));
app.MapGet("/health", async (SalesDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

app.MapGet("/api/sales/orders", async (SalesDbContext db, Guid tenantId, string? search = null, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
{
    var query = db.SalesOrders
        .Include(x => x.Lines)
        .Where(x => x.TenantId == tenantId);

    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.Trim().ToLowerInvariant();
        query = query.Where(x =>
            x.OrderNo.ToLower().Contains(term)
            || x.Status.ToLower().Contains(term)
            || (x.SourceNo != null && x.SourceNo.ToLower().Contains(term))
            || x.Lines.Any(line =>
                line.ProductCode.ToLower().Contains(term)
                || line.Description.ToLower().Contains(term)));
    }

    var total = await query.LongCountAsync(ct);
    var items = await query
        .OrderByDescending(x => x.CreatedAt)
        .Skip(skipCount)
        .Take(maxResultCount)
        .ToArrayAsync(ct);

    return Results.Ok(new { TotalCount = total, Items = items });
});

app.MapGet("/api/sales/orders/{id:guid}", async (Guid id, SalesDbContext db, Guid tenantId, CancellationToken ct) =>
{
    var order = await db.SalesOrders
        .Include(x => x.Lines)
        .SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);

    return order is null ? Results.NotFound() : Results.Ok(order);
});

app.MapPost("/api/sales/orders", async (CreateSalesOrderDto input, SalesDbContext db, NumberingClient numbering, CancellationToken ct) =>
{
    var orderNo = (input.OrderNo ?? string.Empty).Trim().ToUpperInvariant();
    if (string.IsNullOrWhiteSpace(orderNo) || orderNo == "AUTO")
    {
        orderNo = await numbering.GetNextNumberAsync(input.TenantId, "SALES", "ORDER", "SO-", ct);
    }

    if (await db.SalesOrders.AnyAsync(x => x.TenantId == input.TenantId && x.OrderNo == orderNo, ct))
    {
        return Results.Conflict(new { Code = "Sales:OrderAlreadyExists", Message = "Sales order number already exists." });
    }

    var lines = input.Lines.Select(x => new SalesOrderLineDraft(x.WarehouseCode, x.ProductCode, x.Description, x.Quantity, x.UnitPrice, x.DiscountPercent, x.TaxPercent)).ToArray();
    var order = new SalesOrder(
        Guid.NewGuid(),
        input.TenantId,
        input.CustomerId,
        orderNo,
        input.SourceType,
        input.SourceId,
        input.SourceNo,
        lines,
        DateTimeOffset.UtcNow);
    await db.SalesOrders.AddAsync(order, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/sales/orders/{order.Id}", order);
});

app.MapPost("/api/sales/orders/{id:guid}/approve", async (Guid id, SalesDbContext db, InventoryClient inventory, CancellationToken ct) =>
{
    var order = await db.SalesOrders
        .Include(x => x.Lines)
        .SingleOrDefaultAsync(x => x.Id == id, ct);
    if (order is null)
    {
        return Results.NotFound();
    }

    var now = DateTimeOffset.UtcNow;
    if (order.InventoryReservationStatus != "Reserved")
    {
        var reservation = await inventory.ReserveAsync(order, ct);
        if (!reservation.Success)
        {
            return Results.Conflict(new { Code = reservation.Code, Message = reservation.Message });
        }

        order.MarkInventoryReserved(now);
    }

    order.Approve(now);
    await db.SaveChangesAsync(ct);
    return Results.Ok(order);
});

app.MapPost("/api/sales/orders/{id:guid}/complete", async (Guid id, SalesDbContext db, InventoryClient inventory, CancellationToken ct) =>
{
    var order = await db.SalesOrders
        .Include(x => x.Lines)
        .SingleOrDefaultAsync(x => x.Id == id, ct);
    if (order is null)
    {
        return Results.NotFound();
    }

    if (order.DeliveryStatus != "Delivered")
    {
        if (order.InventoryReservationStatus != "Reserved")
        {
            return Results.Conflict(new { Code = "Sales:InventoryNotReserved", Message = "Đơn hàng chưa giữ tồn kho, không thể hoàn tất." });
        }

        var shipment = await inventory.ShipAsync(order, ct);
        if (!shipment.Success)
        {
            return Results.Conflict(new { Code = shipment.Code, Message = shipment.Message });
        }
    }

    order.Complete(DateTimeOffset.UtcNow);
    await db.SaveChangesAsync(ct);
    return Results.Ok(order);
});

app.MapPost("/api/sales/orders/{id:guid}/unapprove", async (Guid id, SalesDbContext db, InventoryClient inventory, CancellationToken ct) =>
{
    var order = await db.SalesOrders
        .Include(x => x.Lines)
        .SingleOrDefaultAsync(x => x.Id == id, ct);
    if (order is null)
    {
        return Results.NotFound();
    }

    if (!order.CanUnapprove)
    {
        return Results.Conflict(new { Code = "Sales:CannotUnapprove", Message = "Chỉ huỷ duyệt được đơn đã duyệt và chưa giao." });
    }

    // Release the held stock first; if the reservation is already shipped this fails and we keep the order approved.
    var release = await inventory.ReleaseAsync(order, ct);
    if (!release.Success)
    {
        return Results.Conflict(new { Code = release.Code, Message = release.Message });
    }

    order.Unapprove();
    await db.SaveChangesAsync(ct);
    return Results.Ok(order);
});

app.MapDelete("/api/sales/orders/{id:guid}", async (Guid id, SalesDbContext db, CancellationToken ct) =>
{
    var order = await db.SalesOrders.SingleOrDefaultAsync(x => x.Id == id, ct);
    if (order is null)
    {
        return Results.NotFound();
    }

    if (!order.CanDelete)
    {
        return Results.Conflict(new { Code = "Sales:CannotDelete", Message = "Chỉ xoá được đơn ở trạng thái nháp." });
    }

    db.SalesOrders.Remove(order);
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

app.MapPost("/api/sales/orders/{id:guid}/deliver", async (Guid id, SalesDbContext db, InventoryClient inventory, CancellationToken ct) =>
{
    var order = await db.SalesOrders
        .Include(x => x.Lines)
        .SingleOrDefaultAsync(x => x.Id == id, ct);
    if (order is null)
    {
        return Results.NotFound();
    }

    if (order.InventoryReservationStatus != "Reserved")
    {
        return Results.Conflict(new { Code = "Sales:InventoryNotReserved", Message = "Đơn hàng chưa giữ tồn kho, không thể giao hàng." });
    }

    var shipment = await inventory.ShipAsync(order, ct);
    if (!shipment.Success)
    {
        return Results.Conflict(new { Code = shipment.Code, Message = shipment.Message });
    }

    order.MarkDelivered(DateTimeOffset.UtcNow);
    await db.SaveChangesAsync(ct);
    return Results.Ok(order);
});

app.MapNexusObservability();
app.Run();

public sealed record CreateSalesOrderDto(Guid TenantId, Guid CustomerId, string OrderNo, string? SourceType, Guid? SourceId, string? SourceNo, IReadOnlyCollection<CreateSalesOrderLineDto> Lines);
public sealed record CreateSalesOrderLineDto(string WarehouseCode, string ProductCode, string Description, decimal Quantity, decimal UnitPrice, decimal DiscountPercent, decimal TaxPercent);

public sealed class InventoryClient
{
    private readonly HttpClient _httpClient;

    public InventoryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<InventoryCallResult> ReserveAsync(SalesOrder order, CancellationToken ct)
    {
        var request = new InventoryReserveRequest(
            order.TenantId,
            "SALES_ORDER",
            order.Id,
            order.OrderNo,
            order.Lines.Select(x => new InventoryLineRequest(x.WarehouseCode, x.ProductCode, x.Description, x.Quantity)).ToArray());

        return PostAsync("/api/inventory/reservations", request, ct);
    }

    public Task<InventoryCallResult> ShipAsync(SalesOrder order, CancellationToken ct)
    {
        var request = new InventoryShipRequest(order.TenantId, "SALES_ORDER", order.Id, order.OrderNo);
        return PostAsync("/api/inventory/shipments", request, ct);
    }

    public Task<InventoryCallResult> ReleaseAsync(SalesOrder order, CancellationToken ct)
    {
        var request = new InventoryShipRequest(order.TenantId, "SALES_ORDER", order.Id, order.OrderNo);
        return PostAsync("/api/inventory/reservations/release", request, ct);
    }

    private async Task<InventoryCallResult> PostAsync(string path, object request, CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(path, request, ct);
            if (response.IsSuccessStatusCode)
            {
                return InventoryCallResult.Ok();
            }

            var error = await response.Content.ReadFromJsonAsync<InventoryErrorResponse>(cancellationToken: ct);
            if (!string.IsNullOrWhiteSpace(error?.Code) || !string.IsNullOrWhiteSpace(error?.Message))
            {
                return InventoryCallResult.Failed(
                    string.IsNullOrWhiteSpace(error?.Code) ? "Inventory:RequestFailed" : error!.Code!,
                    string.IsNullOrWhiteSpace(error?.Message) ? "Inventory request failed." : error!.Message!);
            }

            return InventoryCallResult.Failed("Inventory:RequestFailed", "Inventory request failed.");
        }
        catch (HttpRequestException ex)
        {
            return InventoryCallResult.Failed("Inventory:Unavailable", ex.Message);
        }
        catch (NotSupportedException ex)
        {
            return InventoryCallResult.Failed("Inventory:InvalidErrorResponse", ex.Message);
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

    // Allocates a guaranteed-unique number from the numbering-service. Forwards the caller's
    // bearer token so the (authenticated) tenant user passes the numbering authorization.
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

public sealed record InventoryReserveRequest(Guid TenantId, string SourceType, Guid SourceId, string SourceNo, IReadOnlyCollection<InventoryLineRequest> Lines);
public sealed record InventoryShipRequest(Guid TenantId, string SourceType, Guid SourceId, string SourceNo);
public sealed record InventoryLineRequest(string WarehouseCode, string ProductCode, string Description, decimal Quantity);
public sealed record InventoryErrorResponse(string? Code, string? Message);
public sealed record InventoryCallResult(bool Success, string Code, string Message)
{
    public static InventoryCallResult Ok() => new(true, string.Empty, string.Empty);
    public static InventoryCallResult Failed(string code, string message) => new(false, code, message);
}
