using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.DependencyInjection;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.Crm.Api.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CrmDb")
    ?? "Host=localhost;Port=5432;Database=crm_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddNexusEfCore<CrmDbContext>(connectionString);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus CRM Service", Status = "Running" }));
app.MapGet("/health", async (CrmDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

app.MapGet("/api/crm/customers", async (CrmDbContext db, Guid tenantId, string? search, int skipCount = 0, int maxResultCount = 50, CancellationToken ct = default) =>
{
    var query = db.Customers.Where(x => x.TenantId == tenantId);
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(x => x.Code.Contains(search) || x.Name.Contains(search));
    }

    var total = await query.LongCountAsync(ct);
    var items = await query
        .OrderBy(x => x.Name)
        .Skip(skipCount)
        .Take(maxResultCount)
        .ToArrayAsync(ct);

    return Results.Ok(new { TotalCount = total, Items = items });
});

app.MapPost("/api/crm/customers", async (CreateCustomerDto input, CrmDbContext db, CancellationToken ct) =>
{
    var code = input.Code.Trim().ToUpperInvariant();
    if (await db.Customers.AnyAsync(x => x.TenantId == input.TenantId && x.Code == code, ct))
    {
        return Results.Conflict(new { Code = "Crm:CustomerAlreadyExists", Message = "Customer code already exists." });
    }

    var customer = new Customer(Guid.NewGuid(), input.TenantId, code, input.Name, input.Email, input.Phone, DateTimeOffset.UtcNow);
    await db.Customers.AddAsync(customer, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/crm/customers/{customer.Id}", customer);
});

app.MapPost("/api/crm/leads", async (CreateLeadDto input, CrmDbContext db, CancellationToken ct) =>
{
    var lead = new Lead(Guid.NewGuid(), input.TenantId, input.FullName, input.CompanyName, input.Email, input.Phone, input.Source, DateTimeOffset.UtcNow);
    await db.Leads.AddAsync(lead, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/crm/leads/{lead.Id}", lead);
});

app.MapGet("/api/crm/leads", async (CrmDbContext db, Guid tenantId, CancellationToken ct) =>
{
    var items = await db.Leads
        .Where(x => x.TenantId == tenantId)
        .OrderByDescending(x => x.CreatedAt)
        .ToArrayAsync(ct);

    return Results.Ok(items);
});

app.MapPost("/api/crm/opportunities", async (CreateOpportunityDto input, CrmDbContext db, CancellationToken ct) =>
{
    var opportunity = new Opportunity(Guid.NewGuid(), input.TenantId, input.CustomerId, input.Name, input.Amount, input.ExpectedCloseDate, DateTimeOffset.UtcNow);
    await db.Opportunities.AddAsync(opportunity, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/crm/opportunities/{opportunity.Id}", opportunity);
});

app.MapGet("/api/crm/opportunities", async (CrmDbContext db, Guid tenantId, CancellationToken ct) =>
{
    var items = await db.Opportunities
        .Where(x => x.TenantId == tenantId)
        .OrderByDescending(x => x.CreatedAt)
        .ToArrayAsync(ct);

    return Results.Ok(items);
});

app.MapPost("/api/crm/quotations", async (CreateQuotationDto input, CrmDbContext db, CancellationToken ct) =>
{
    var quotationNo = input.QuotationNo.Trim().ToUpperInvariant();
    if (await db.Quotations.AnyAsync(x => x.TenantId == input.TenantId && x.QuotationNo == quotationNo, ct))
    {
        return Results.Conflict(new { Code = "Crm:QuotationAlreadyExists", Message = "Quotation number already exists." });
    }

    var quotation = new Quotation(Guid.NewGuid(), input.TenantId, input.CustomerId, quotationNo, input.TotalAmount, DateTimeOffset.UtcNow);
    await db.Quotations.AddAsync(quotation, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/crm/quotations/{quotation.Id}", quotation);
});

app.MapPost("/api/crm/quotations/{id:guid}/approve", async (Guid id, CrmDbContext db, CancellationToken ct) =>
{
    var quotation = await db.Quotations.FindAsync([id], ct);
    if (quotation is null)
    {
        return Results.NotFound();
    }

    quotation.Approve(DateTimeOffset.UtcNow);
    await db.SaveChangesAsync(ct);
    return Results.Ok(quotation);
});

app.MapGet("/api/crm/quotations", async (CrmDbContext db, Guid tenantId, CancellationToken ct) =>
{
    var items = await db.Quotations
        .Where(x => x.TenantId == tenantId)
        .OrderByDescending(x => x.CreatedAt)
        .ToArrayAsync(ct);

    return Results.Ok(items);
});

app.MapPost("/api/crm/contracts", async (CreateContractDto input, CrmDbContext db, CancellationToken ct) =>
{
    var contractNo = input.ContractNo.Trim().ToUpperInvariant();
    if (await db.Contracts.AnyAsync(x => x.TenantId == input.TenantId && x.ContractNo == contractNo, ct))
    {
        return Results.Conflict(new { Code = "Crm:ContractAlreadyExists", Message = "Contract number already exists." });
    }

    var contract = new Contract(Guid.NewGuid(), input.TenantId, input.CustomerId, contractNo, input.Title, DateTimeOffset.UtcNow);
    await db.Contracts.AddAsync(contract, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/crm/contracts/{contract.Id}", contract);
});

app.MapPost("/api/crm/contracts/{id:guid}/sign", async (Guid id, CrmDbContext db, CancellationToken ct) =>
{
    var contract = await db.Contracts.FindAsync([id], ct);
    if (contract is null)
    {
        return Results.NotFound();
    }

    contract.Sign(DateTimeOffset.UtcNow);
    await db.SaveChangesAsync(ct);
    return Results.Ok(contract);
});

app.MapGet("/api/crm/contracts", async (CrmDbContext db, Guid tenantId, CancellationToken ct) =>
{
    var items = await db.Contracts
        .Where(x => x.TenantId == tenantId)
        .OrderByDescending(x => x.CreatedAt)
        .ToArrayAsync(ct);

    return Results.Ok(items);
});

app.Run();

public sealed record CreateCustomerDto(Guid TenantId, string Code, string Name, string? Email, string? Phone);
public sealed record CreateLeadDto(Guid TenantId, string FullName, string? CompanyName, string? Email, string? Phone, string? Source);
public sealed record CreateOpportunityDto(Guid TenantId, Guid? CustomerId, string Name, decimal Amount, DateOnly? ExpectedCloseDate);
public sealed record CreateQuotationDto(Guid TenantId, Guid CustomerId, string QuotationNo, decimal TotalAmount);
public sealed record CreateContractDto(Guid TenantId, Guid CustomerId, string ContractNo, string Title);
