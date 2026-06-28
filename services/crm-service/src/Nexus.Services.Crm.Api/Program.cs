using Nexus.BuildingBlocks.Observability;
using Microsoft.EntityFrameworkCore;
using Nexus.ApiContracts.Permissions;
using Nexus.BuildingBlocks.Web.Auth;
using Nexus.BuildingBlocks.Web.DependencyInjection;
using Nexus.Services.Crm.Application.DependencyInjection;
using Nexus.Services.Crm.Contracts.Activities;
using Nexus.Services.Crm.Contracts.Contacts;
using Nexus.Services.Crm.Contracts.Contracts;
using Nexus.Services.Crm.Contracts.Customers;
using Nexus.Services.Crm.Contracts.Dashboard;
using Nexus.Services.Crm.Contracts.Leads;
using Nexus.Services.Crm.Contracts.Opportunities;
using Nexus.Services.Crm.Contracts.Quotations;
using Nexus.Services.Crm.Infrastructure.DependencyInjection;
using Nexus.Services.Crm.Infrastructure.Persistence;
using Nexus.Services.Crm.Domain.Enums;
using Nexus.SharedKernel.Exceptions;

var builder = WebApplication.CreateBuilder(args);
builder.AddNexusObservability("crm-service");

var connectionString = builder.Configuration.GetConnectionString("CrmDb")
    ?? "Host=localhost;Port=5432;Database=crm_db;Username=nexus;Password=nexus_dev_password";

builder.Services.AddNexusWeb();
builder.Services.AddNexusJwtAuth(builder.Configuration);
builder.Services.AddCrmInfrastructure(connectionString);
builder.Services.AddCrmApplication();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus CRM Service", Status = "Running" }));
app.MapGet("/health", async (CrmDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return ok ? Results.Ok(new { Status = "Healthy" }) : Results.StatusCode(503);
});

var crm = app.MapGroup("/api/crm").RequireAuthorization();

// Customers
crm.MapGet("/customers", async (string? search, int skipCount, int maxResultCount, string? sorting, CustomerStatus? status, Guid? ownerId, ICustomerAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.GetListAsync(new GetCustomersInput { Search = search, Status = status, OwnerId = ownerId, SkipCount = skipCount, MaxResultCount = maxResultCount, Sorting = sorting }, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Customers.View));
crm.MapGet("/customers/{id:guid}", async (Guid id, ICustomerAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.GetAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Customers.View));
crm.MapPost("/customers", async (CreateCustomerDto input, ICustomerAppService service, CancellationToken ct) =>
    await ExecuteCreatedAsync($"/api/crm/customers/{{0}}", () => service.CreateAsync(input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Customers.Create));
crm.MapPut("/customers/{id:guid}", async (Guid id, UpdateCustomerDto input, ICustomerAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.UpdateAsync(id, input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Customers.Edit));
crm.MapDelete("/customers/{id:guid}", async (Guid id, ICustomerAppService service, CancellationToken ct) =>
    await ExecuteDeleteAsync(() => service.DeleteAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Customers.Delete));

// Contacts
crm.MapGet("/contacts", async (Guid? customerId, string? search, int skipCount, int maxResultCount, string? sorting, IContactAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.GetListAsync(new GetContactsInput { CustomerId = customerId, Search = search, SkipCount = skipCount, MaxResultCount = maxResultCount, Sorting = sorting }, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Contacts.View));
crm.MapGet("/contacts/{id:guid}", async (Guid id, IContactAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.GetAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Contacts.View));
crm.MapPost("/contacts", async (CreateContactDto input, IContactAppService service, CancellationToken ct) =>
    await ExecuteCreatedAsync("/api/crm/contacts/{0}", () => service.CreateAsync(input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Contacts.Create));
crm.MapPut("/contacts/{id:guid}", async (Guid id, UpdateContactDto input, IContactAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.UpdateAsync(id, input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Contacts.Edit));
crm.MapDelete("/contacts/{id:guid}", async (Guid id, IContactAppService service, CancellationToken ct) =>
    await ExecuteDeleteAsync(() => service.DeleteAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Contacts.Delete));

// Leads
crm.MapGet("/leads", async (string? search, LeadStatus? status, Guid? ownerId, int skipCount, int maxResultCount, string? sorting, ILeadAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.GetListAsync(new GetLeadsInput { Search = search, Status = status, OwnerId = ownerId, SkipCount = skipCount, MaxResultCount = maxResultCount, Sorting = sorting }, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Leads.View));
crm.MapGet("/leads/{id:guid}", async (Guid id, ILeadAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.GetAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Leads.View));
crm.MapPost("/leads", async (CreateLeadDto input, ILeadAppService service, CancellationToken ct) =>
    await ExecuteCreatedAsync("/api/crm/leads/{0}", () => service.CreateAsync(input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Leads.Create));
crm.MapPut("/leads/{id:guid}", async (Guid id, UpdateLeadDto input, ILeadAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.UpdateAsync(id, input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Leads.Edit));
crm.MapDelete("/leads/{id:guid}", async (Guid id, ILeadAppService service, CancellationToken ct) =>
    await ExecuteDeleteAsync(() => service.DeleteAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Leads.Delete));
crm.MapPost("/leads/{id:guid}/convert", async (Guid id, ConvertLeadDto input, ILeadAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.ConvertAsync(id, input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Leads.Edit));

// Opportunities
crm.MapGet("/opportunities", async (string? search, OpportunityStage? stage, Guid? customerId, Guid? ownerId, int skipCount, int maxResultCount, string? sorting, IOpportunityAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.GetListAsync(new GetOpportunitiesInput { Search = search, Stage = stage, CustomerId = customerId, OwnerId = ownerId, SkipCount = skipCount, MaxResultCount = maxResultCount, Sorting = sorting }, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Opportunities.View));
crm.MapGet("/opportunities/{id:guid}", async (Guid id, IOpportunityAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.GetAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Opportunities.View));
crm.MapPost("/opportunities", async (CreateOpportunityDto input, IOpportunityAppService service, CancellationToken ct) =>
    await ExecuteCreatedAsync("/api/crm/opportunities/{0}", () => service.CreateAsync(input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Opportunities.Create));
crm.MapPut("/opportunities/{id:guid}", async (Guid id, UpdateOpportunityDto input, IOpportunityAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.UpdateAsync(id, input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Opportunities.Edit));
crm.MapDelete("/opportunities/{id:guid}", async (Guid id, IOpportunityAppService service, CancellationToken ct) =>
    await ExecuteDeleteAsync(() => service.DeleteAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Opportunities.Delete));
crm.MapPatch("/opportunities/{id:guid}/stage", async (Guid id, ChangeOpportunityStageDto input, IOpportunityAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.ChangeStageAsync(id, input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.OpportunityBoard.Edit));

// Quotations
crm.MapGet("/quotations", async (string? search, QuotationStatus? status, Guid? customerId, int skipCount, int maxResultCount, string? sorting, IQuotationAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.GetListAsync(new GetQuotationsInput { Search = search, Status = status, CustomerId = customerId, SkipCount = skipCount, MaxResultCount = maxResultCount, Sorting = sorting }, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Quotations.View));
crm.MapGet("/quotations/{id:guid}", async (Guid id, IQuotationAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.GetAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Quotations.View));
crm.MapPost("/quotations", async (CreateQuotationDto input, IQuotationAppService service, CancellationToken ct) =>
    await ExecuteCreatedAsync("/api/crm/quotations/{0}", () => service.CreateAsync(input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Quotations.Create));
crm.MapPut("/quotations/{id:guid}", async (Guid id, UpdateQuotationDto input, IQuotationAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.UpdateAsync(id, input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Quotations.Edit));
crm.MapDelete("/quotations/{id:guid}", async (Guid id, IQuotationAppService service, CancellationToken ct) =>
    await ExecuteDeleteAsync(() => service.DeleteAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Quotations.Delete));
crm.MapPost("/quotations/{id:guid}/approve", async (Guid id, IQuotationAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.ApproveAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Quotations.Approve));
crm.MapPost("/quotations/{id:guid}/reject", async (Guid id, RejectQuotationDto input, IQuotationAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.RejectAsync(id, input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Quotations.Approve));
crm.MapPost("/quotations/{id:guid}/send", async (Guid id, IQuotationAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.SendAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Quotations.Edit));

// Contracts
crm.MapGet("/contracts", async (string? search, ContractStatus? status, Guid? customerId, int skipCount, int maxResultCount, string? sorting, IContractAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.GetListAsync(new GetContractsInput { Search = search, Status = status, CustomerId = customerId, SkipCount = skipCount, MaxResultCount = maxResultCount, Sorting = sorting }, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Contracts.View));
crm.MapGet("/contracts/{id:guid}", async (Guid id, IContractAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.GetAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Contracts.View));
crm.MapPost("/contracts", async (CreateContractDto input, IContractAppService service, CancellationToken ct) =>
    await ExecuteCreatedAsync("/api/crm/contracts/{0}", () => service.CreateAsync(input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Contracts.Create));
crm.MapPut("/contracts/{id:guid}", async (Guid id, UpdateContractDto input, IContractAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.UpdateAsync(id, input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Contracts.Edit));
crm.MapDelete("/contracts/{id:guid}", async (Guid id, IContractAppService service, CancellationToken ct) =>
    await ExecuteDeleteAsync(() => service.DeleteAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Contracts.Delete));
crm.MapPost("/contracts/{id:guid}/sign", async (Guid id, IContractAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.SignAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Contracts.Sign));
crm.MapPost("/contracts/{id:guid}/activate", async (Guid id, IContractAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.ActivateAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Contracts.Edit));
crm.MapPost("/contracts/{id:guid}/complete", async (Guid id, IContractAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.CompleteAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Contracts.Edit));
crm.MapPost("/contracts/{id:guid}/terminate", async (Guid id, TerminateContractDto input, IContractAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.TerminateAsync(id, input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Contracts.Edit));

// Activities
crm.MapGet("/activities", async (CrmRelatedEntityType? relatedEntityType, Guid? relatedEntityId, CrmActivityStatus? status, int skipCount, int maxResultCount, string? sorting, IActivityAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.GetListAsync(new GetActivitiesInput { RelatedEntityType = relatedEntityType, RelatedEntityId = relatedEntityId, Status = status, SkipCount = skipCount, MaxResultCount = maxResultCount, Sorting = sorting }, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Activities.View));
crm.MapGet("/activities/{id:guid}", async (Guid id, IActivityAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.GetAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Activities.View));
crm.MapPost("/activities", async (CreateActivityDto input, IActivityAppService service, CancellationToken ct) =>
    await ExecuteCreatedAsync("/api/crm/activities/{0}", () => service.CreateAsync(input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Activities.Create));
crm.MapPut("/activities/{id:guid}", async (Guid id, UpdateActivityDto input, IActivityAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.UpdateAsync(id, input, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Activities.Edit));
crm.MapDelete("/activities/{id:guid}", async (Guid id, IActivityAppService service, CancellationToken ct) =>
    await ExecuteDeleteAsync(() => service.DeleteAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Activities.Delete));
crm.MapPost("/activities/{id:guid}/complete", async (Guid id, IActivityAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.CompleteAsync(id, ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Activities.Complete));

// Dashboard
crm.MapGet("/dashboard", async (ICrmDashboardAppService service, CancellationToken ct) =>
    await ExecuteAsync(() => service.GetAsync(ct)))
    .RequireAuthorization(NexusPolicies.Permission(NexusPermissions.Crm.Dashboard.View));

app.MapNexusObservability();
app.Run();

static async Task<IResult> ExecuteAsync<T>(Func<Task<T>> action)
{
    try
    {
        return Results.Ok(await action());
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { Message = ex.Message });
    }
    catch (NexusBusinessException ex)
    {
        return Results.Conflict(new { ex.Code, ex.Message });
    }
}

static async Task<IResult> ExecuteCreatedAsync<T>(string urlTemplate, Func<Task<T>> action) where T : notnull
{
    try
    {
        var result = await action();
        var id = result.GetType().GetProperty("Id")?.GetValue(result);
        return Results.Created(string.Format(urlTemplate, id), result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { Message = ex.Message });
    }
    catch (NexusBusinessException ex)
    {
        return Results.Conflict(new { ex.Code, ex.Message });
    }
}

static async Task<IResult> ExecuteDeleteAsync(Func<Task> action)
{
    try
    {
        await action();
        return Results.NoContent();
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { Message = ex.Message });
    }
    catch (NexusBusinessException ex)
    {
        return Results.Conflict(new { ex.Code, ex.Message });
    }
}
