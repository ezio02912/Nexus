using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.Sales.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SalesDb")
    ?? "Host=localhost;Port=5432;Database=sales_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddNexusEfCore<SalesDbContext>(connectionString);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus Sales Service", Status = "Running" }));
app.MapGet("/health", async (SalesDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

app.MapGet("/api/sales/orders", async (SalesDbContext db, Guid tenantId, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
{
    var query = db.SalesOrders
        .Include(x => x.Lines)
        .Where(x => x.TenantId == tenantId);

    var total = await query.LongCountAsync(ct);
    var items = await query
        .OrderByDescending(x => x.CreatedAt)
        .Skip(skipCount)
        .Take(maxResultCount)
        .ToArrayAsync(ct);

    return Results.Ok(new { TotalCount = total, Items = items });
});

app.MapPost("/api/sales/orders", async (CreateSalesOrderDto input, SalesDbContext db, CancellationToken ct) =>
{
    var orderNo = input.OrderNo.Trim().ToUpperInvariant();
    if (await db.SalesOrders.AnyAsync(x => x.TenantId == input.TenantId && x.OrderNo == orderNo, ct))
    {
        return Results.Conflict(new { Code = "Sales:OrderAlreadyExists", Message = "Sales order number already exists." });
    }

    var lines = input.Lines.Select(x => new SalesOrderLineDraft(x.ProductCode, x.Description, x.Quantity, x.UnitPrice)).ToArray();
    var order = new SalesOrder(Guid.NewGuid(), input.TenantId, input.CustomerId, orderNo, lines, DateTimeOffset.UtcNow);
    await db.SalesOrders.AddAsync(order, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/sales/orders/{order.Id}", order);
});

app.MapPost("/api/sales/orders/{id:guid}/approve", async (Guid id, SalesDbContext db, CancellationToken ct) =>
{
    var order = await db.SalesOrders.FindAsync([id], ct);
    if (order is null)
    {
        return Results.NotFound();
    }

    order.Approve(DateTimeOffset.UtcNow);
    await db.SaveChangesAsync(ct);
    return Results.Ok(order);
});

app.MapPost("/api/sales/orders/{id:guid}/complete", async (Guid id, SalesDbContext db, CancellationToken ct) =>
{
    var order = await db.SalesOrders.FindAsync([id], ct);
    if (order is null)
    {
        return Results.NotFound();
    }

    order.Complete(DateTimeOffset.UtcNow);
    await db.SaveChangesAsync(ct);
    return Results.Ok(order);
});

app.Run();

public sealed record CreateSalesOrderDto(Guid TenantId, Guid CustomerId, string OrderNo, IReadOnlyCollection<CreateSalesOrderLineDto> Lines);
public sealed record CreateSalesOrderLineDto(string ProductCode, string Description, decimal Quantity, decimal UnitPrice);
