using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Nexus.Web.Tenant.Services;

public sealed class TenantPortalApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TenantPortalOptions _options;

    public TenantPortalApiClient(IHttpClientFactory httpClientFactory, IOptions<TenantPortalOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public Task<TenantDto?> GetTenantAsync(Guid tenantId) => GetAsync<TenantDto>(_options.Tenant, $"/api/tenants/{tenantId}");
    public Task<TenantDto?> GetTenantByCodeAsync(string code) => GetAsync<TenantDto>(_options.Tenant, $"/api/public/tenants/by-code/{Uri.EscapeDataString(code.Trim())}");
    public Task<LoginResult?> LoginAsync(LoginRequest request) => PostAsync<LoginResult>(_options.Identity, "/api/auth/login", request);
    public Task<PagedResult<UserDto>?> GetUsersAsync(Guid? tenantId = null)
    {
        var path = tenantId.HasValue ? $"/api/users/?tenantId={tenantId}" : "/api/users/";
        return GetAsync<PagedResult<UserDto>>(_options.Identity, path);
    }
    public Task<UserDto?> CreateUserAsync(CreateUserRequest request) => PostAsync<UserDto>(_options.Identity, "/api/users/", request);
    public Task<IReadOnlyList<string>?> GetPermissionCatalogAsync() => GetAsync<IReadOnlyList<string>>(_options.Permission, "/api/permissions");
    public Task<RolePermissionDto?> GetRolePermissionsAsync(string roleName) => GetAsync<RolePermissionDto>(_options.Permission, $"/api/roles/{Uri.EscapeDataString(roleName)}/permissions");
    public Task<RolePermissionDto?> UpdateRolePermissionsAsync(string roleName, UpdateRolePermissionsRequest request) => PutAsync<RolePermissionDto>(_options.Permission, $"/api/roles/{Uri.EscapeDataString(roleName)}/permissions", request);
    public Task<PagedResult<CustomerRecord>?> GetCustomersAsync(Guid tenantId) => GetAsync<PagedResult<CustomerRecord>>(_options.Crm, $"/api/crm/customers?tenantId={tenantId}");
    public Task<CustomerRecord?> CreateCustomerAsync(CreateCustomerRequest request) => PostAsync<CustomerRecord>(_options.Crm, "/api/crm/customers", request);
    public Task<IReadOnlyList<LeadRecord>?> GetLeadsAsync(Guid tenantId) => GetAsync<IReadOnlyList<LeadRecord>>(_options.Crm, $"/api/crm/leads?tenantId={tenantId}");
    public Task<LeadRecord?> CreateLeadAsync(CreateLeadRequest request) => PostAsync<LeadRecord>(_options.Crm, "/api/crm/leads", request);
    public Task<IReadOnlyList<OpportunityRecord>?> GetOpportunitiesAsync(Guid tenantId) => GetAsync<IReadOnlyList<OpportunityRecord>>(_options.Crm, $"/api/crm/opportunities?tenantId={tenantId}");
    public Task<OpportunityRecord?> CreateOpportunityAsync(CreateOpportunityRequest request) => PostAsync<OpportunityRecord>(_options.Crm, "/api/crm/opportunities", request);
    public Task<IReadOnlyList<QuotationRecord>?> GetQuotationsAsync(Guid tenantId) => GetAsync<IReadOnlyList<QuotationRecord>>(_options.Crm, $"/api/crm/quotations?tenantId={tenantId}");
    public Task<QuotationRecord?> CreateQuotationAsync(CreateQuotationRequest request) => PostAsync<QuotationRecord>(_options.Crm, "/api/crm/quotations", request);
    public Task<QuotationRecord?> ApproveQuotationAsync(Guid id) => PostAsync<QuotationRecord>(_options.Crm, $"/api/crm/quotations/{id}/approve", new { });
    public Task<IReadOnlyList<ContractRecord>?> GetContractsAsync(Guid tenantId) => GetAsync<IReadOnlyList<ContractRecord>>(_options.Crm, $"/api/crm/contracts?tenantId={tenantId}");
    public Task<ContractRecord?> CreateContractAsync(CreateContractRequest request) => PostAsync<ContractRecord>(_options.Crm, "/api/crm/contracts", request);
    public Task<ContractRecord?> SignContractAsync(Guid id) => PostAsync<ContractRecord>(_options.Crm, $"/api/crm/contracts/{id}/sign", new { });
    public Task<PagedResult<SalesOrderRecord>?> GetSalesOrdersAsync(Guid tenantId) => GetAsync<PagedResult<SalesOrderRecord>>(_options.Sales, $"/api/sales/orders?tenantId={tenantId}");
    public Task<SalesOrderRecord?> CreateSalesOrderAsync(CreateSalesOrderRequest request) => PostAsync<SalesOrderRecord>(_options.Sales, "/api/sales/orders", request);
    public Task<SalesOrderRecord?> ApproveSalesOrderAsync(Guid id) => PostAsync<SalesOrderRecord>(_options.Sales, $"/api/sales/orders/{id}/approve", new { });
    public Task<SalesOrderRecord?> CompleteSalesOrderAsync(Guid id) => PostAsync<SalesOrderRecord>(_options.Sales, $"/api/sales/orders/{id}/complete", new { });

    private async Task<T?> GetAsync<T>(string baseUrl, string path)
    {
        return await CreateClient(baseUrl).GetFromJsonAsync<T>(path);
    }

    private async Task<T?> PostAsync<T>(string baseUrl, string path, object request)
    {
        var response = await CreateClient(baseUrl).PostAsJsonAsync(path, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    private async Task<T?> PutAsync<T>(string baseUrl, string path, object request)
    {
        var response = await CreateClient(baseUrl).PutAsJsonAsync(path, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    private HttpClient CreateClient(string baseUrl)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/'));
        return client;
    }
}
