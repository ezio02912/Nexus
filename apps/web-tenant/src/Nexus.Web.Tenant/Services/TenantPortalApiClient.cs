using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Nexus.Web.Tenant.Services;

public sealed class TenantPortalApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TenantPortalOptions _options;
    private readonly TenantSessionService _session;

    public TenantPortalApiClient(
        IHttpClientFactory httpClientFactory,
        IOptions<TenantPortalOptions> options,
        TenantSessionService session)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _session = session;
    }

    public async Task<TenantDto?> GetTenantAsync(Guid tenantId)
    {
        if (_session.IsAuthenticated)
        {
            return await GetAsync<TenantDto>(_options.Tenant, $"/api/tenants/{tenantId}");
        }

        var response = await CreateClient().GetAsync(
            BuildUrl(_options.Tenant, $"/api/public/tenants/by-id/{tenantId}"));
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TenantDto>();
    }

    public async Task<TenantDto?> GetTenantByCodeAsync(string code)
    {
        var normalized = code.Trim();
        var response = await CreateClient().GetAsync(
            BuildUrl(_options.Tenant, $"/api/public/tenants/by-code/{Uri.EscapeDataString(normalized)}"));

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TenantDto>();
    }

    public Task<LoginResult?> LoginAsync(LoginRequest request) => PostPublicAsync<LoginResult>(_options.Identity, "/api/auth/login", request);

    public Task<LoginResult?> LoginEmailAsync(LoginEmailRequest request) => PostPublicAsync<LoginResult>(_options.Identity, "/api/auth/login-email", request);

    public Task<GoogleAuthResult?> GoogleAuthAsync(GoogleAuthRequest request) => PostPublicAsync<GoogleAuthResult>(_options.Identity, "/api/auth/google", request);

    public Task<PreviewTenantCodeResult?> PreviewTenantCodeAsync(PreviewTenantCodeRequest request) =>
        PostPublicAsync<PreviewTenantCodeResult>(_options.Identity, "/api/onboarding/preview-code", request);

    public Task<CompleteOnboardingResult?> CompleteOnboardingAsync(CompleteOnboardingRequest request) =>
        PostPublicAsync<CompleteOnboardingResult>(_options.Identity, "/api/onboarding/complete", request);

    public async Task SignInFromLoginAsync(LoginResult login)
    {
        TenantDto? tenant;
        if (!string.IsNullOrWhiteSpace(login.TenantCode))
        {
            tenant = await GetTenantByCodeAsync(login.TenantCode);
        }
        else
        {
            tenant = await GetTenantAsync(login.TenantId);
        }

        if (tenant is null)
        {
            throw new InvalidOperationException("Không thể tải thông tin tenant sau đăng nhập.");
        }

        var userName = login.UserName ?? login.Email ?? tenant.Code;
        await _session.SignInAsync(userName, login, tenant);
    }

    public Task<PagedResult<UserDto>?> GetUsersAsync(Guid? tenantId = null)
    {
        var path = tenantId.HasValue ? $"/api/users/?tenantId={tenantId}" : "/api/users/";
        return GetAsync<PagedResult<UserDto>>(_options.Identity, path);
    }

    public Task<UserDto?> CreateUserAsync(CreateUserRequest request) => PostAsync<UserDto>(_options.Identity, "/api/users/", request);
    public Task<IReadOnlyList<string>?> GetPermissionCatalogAsync() => GetAsync<IReadOnlyList<string>>(_options.Permission, "/api/permissions");
    public Task<RolePermissionDto?> GetRolePermissionsAsync(string roleName) => GetAsync<RolePermissionDto>(_options.Permission, $"/api/roles/{Uri.EscapeDataString(roleName)}/permissions");
    public Task<RolePermissionDto?> UpdateRolePermissionsAsync(string roleName, UpdateRolePermissionsRequest request) => PutAsync<RolePermissionDto>(_options.Permission, $"/api/roles/{Uri.EscapeDataString(roleName)}/permissions", request);
    public Task<PagedResult<SalesOrderRecord>?> GetSalesOrdersAsync(Guid tenantId, string? search = null)
    {
        var path = $"/api/sales/orders?tenantId={tenantId}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            path += $"&search={Uri.EscapeDataString(search)}";
        }

        return GetAsync<PagedResult<SalesOrderRecord>>(_options.Sales, path);
    }
    public Task<SalesOrderRecord?> CreateSalesOrderAsync(CreateSalesOrderRequest request) => PostAsync<SalesOrderRecord>(_options.Sales, "/api/sales/orders", request);
    public Task<SalesOrderRecord?> GetSalesOrderAsync(Guid tenantId, Guid id) => GetAsync<SalesOrderRecord>(_options.Sales, $"/api/sales/orders/{id}?tenantId={tenantId}");
    public Task<SalesOrderRecord?> ApproveSalesOrderAsync(Guid id) => PostAsync<SalesOrderRecord>(_options.Sales, $"/api/sales/orders/{id}/approve", new { });
    public Task<SalesOrderRecord?> DeliverSalesOrderAsync(Guid id) => PostAsync<SalesOrderRecord>(_options.Sales, $"/api/sales/orders/{id}/deliver", new { });
    public Task<SalesOrderRecord?> CompleteSalesOrderAsync(Guid id) => PostAsync<SalesOrderRecord>(_options.Sales, $"/api/sales/orders/{id}/complete", new { });

    public Task<IReadOnlyList<StockBalanceRecord>?> GetStockBalancesAsync(Guid tenantId, string? search = null)
    {
        var path = $"/api/inventory/balances?tenantId={tenantId}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            path += $"&search={Uri.EscapeDataString(search)}";
        }

        return GetAsync<IReadOnlyList<StockBalanceRecord>>(_options.Inventory, path);
    }

    public Task<StockBalanceRecord?> ImportStockAsync(ImportStockRequest request) =>
        PostAsync<StockBalanceRecord>(_options.Inventory, "/api/inventory/stock/import", request);

    public Task<IReadOnlyList<InventoryProductRecord>?> GetInventoryProductsAsync(Guid tenantId, string? search = null)
    {
        var path = $"/api/inventory/products?tenantId={tenantId}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            path += $"&search={Uri.EscapeDataString(search)}";
        }

        return GetAsync<IReadOnlyList<InventoryProductRecord>>(_options.Inventory, path);
    }

    public Task<InventoryProductRecord?> UpsertInventoryProductAsync(UpsertInventoryProductRequest request) =>
        PostAsync<InventoryProductRecord>(_options.Inventory, "/api/inventory/products", request);

    public Task<IReadOnlyList<WarehouseRecord>?> GetWarehousesAsync(Guid tenantId, string? search = null)
    {
        var path = $"/api/inventory/warehouses?tenantId={tenantId}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            path += $"&search={Uri.EscapeDataString(search)}";
        }

        return GetAsync<IReadOnlyList<WarehouseRecord>>(_options.Inventory, path);
    }

    public Task<WarehouseRecord?> UpsertWarehouseAsync(UpsertWarehouseRequest request) =>
        PostAsync<WarehouseRecord>(_options.Inventory, "/api/inventory/warehouses", request);

    public Task<IReadOnlyList<SupplierRecord>?> GetSuppliersAsync(Guid tenantId, string? search = null)
    {
        var path = $"/api/purchase/suppliers?tenantId={tenantId}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            path += $"&search={Uri.EscapeDataString(search)}";
        }

        return GetAsync<IReadOnlyList<SupplierRecord>>(_options.Purchase, path);
    }

    public Task<SupplierRecord?> UpsertSupplierAsync(UpsertSupplierRequest request) =>
        PostAsync<SupplierRecord>(_options.Purchase, "/api/purchase/suppliers", request);

    public Task<IReadOnlyList<PurchaseOrderRecord>?> GetPurchaseOrdersAsync(Guid tenantId, string? search = null)
    {
        var path = $"/api/purchase/orders?tenantId={tenantId}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            path += $"&search={Uri.EscapeDataString(search)}";
        }

        return GetAsync<IReadOnlyList<PurchaseOrderRecord>>(_options.Purchase, path);
    }

    public Task<PurchaseOrderRecord?> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request) =>
        PostAsync<PurchaseOrderRecord>(_options.Purchase, "/api/purchase/orders", request);

    public Task<PurchaseOrderRecord?> ApprovePurchaseOrderAsync(Guid id) =>
        PostAsync<PurchaseOrderRecord>(_options.Purchase, $"/api/purchase/orders/{id}/approve", new { });

    public Task<PurchaseOrderRecord?> ReceivePurchaseOrderAsync(Guid id, ReceivePurchaseOrderRequest request) =>
        PostAsync<PurchaseOrderRecord>(_options.Purchase, $"/api/purchase/orders/{id}/receive", request);

    public Task<IReadOnlyList<GoodsReceiptRecord>?> GetGoodsReceiptsAsync(Guid tenantId) =>
        GetAsync<IReadOnlyList<GoodsReceiptRecord>>(_options.Purchase, $"/api/purchase/goods-receipts?tenantId={tenantId}");

    public Task<IReadOnlyList<SubscriptionPlanDto>?> GetSubscriptionPlansAsync() =>
        GetAsync<IReadOnlyList<SubscriptionPlanDto>>(_options.Tenant, "/api/subscription-plans/");

    public Task<CheckoutSessionDto?> CreateCheckoutAsync(CreateCheckoutRequest request) =>
        PostAsync<CheckoutSessionDto>(_options.Tenant, "/api/billing/checkout", request);

    public Task<TenantSubscriptionDto?> ConfirmCheckoutAsync(Guid checkoutId) =>
        PostAsync<TenantSubscriptionDto>(_options.Tenant, $"/api/billing/checkout/{checkoutId}/confirm", new { });

    public Task<IReadOnlyList<SubscriptionPaymentDto>?> GetBillingInvoicesAsync() =>
        GetAsync<IReadOnlyList<SubscriptionPaymentDto>>(_options.Tenant, "/api/billing/invoices");

    public async Task RefreshTenantSessionAsync()
    {
        if (_session.TenantId is not Guid tenantId || _session.Login is null)
        {
            return;
        }

        var tenant = await GetTenantAsync(tenantId);
        if (tenant is not null && _session.UserName is not null)
        {
            await _session.UpdateTenantAsync(tenant);
        }
    }

    private async Task<T?> GetAsync<T>(string baseUrl, string path)
    {
        var response = await CreateClient().GetAsync(BuildUrl(baseUrl, path));
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    private async Task<T?> PostPublicAsync<T>(string baseUrl, string path, object request)
    {
        var response = await CreateClient().PostAsJsonAsync(BuildUrl(baseUrl, path), request);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return default;
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(body) ? "Yêu cầu thất bại." : body);
        }

        return await response.Content.ReadFromJsonAsync<T>();
    }

    private async Task<T?> PostAsync<T>(string baseUrl, string path, object request)
    {
        var response = await CreateClient().PostAsJsonAsync(BuildUrl(baseUrl, path), request);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return default;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await ReadErrorMessageAsync(response));
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            if (error?.Code is { Length: > 0 } code && TranslateErrorCode(code) is { } translated)
            {
                return translated;
            }

            if (!string.IsNullOrWhiteSpace(error?.Message))
            {
                return error!.Message!;
            }
        }
        catch
        {
            // Fall through to the generic message when the body is not JSON.
        }

        return $"Yêu cầu không thực hiện được ({(int)response.StatusCode} {response.StatusCode}). Vui lòng kiểm tra service/API gateway.";
    }

    private static string? TranslateErrorCode(string code) => code switch
    {
        "Billing.UpgradeNotAllowed" => "Chỉ được nâng cấp lên gói cao hơn.",
        "Billing.CheckoutNotFound" => "Phiên thanh toán không hợp lệ hoặc đã hết hạn.",
        "Billing.InvalidPlan" => "Gói được chọn không yêu cầu thanh toán.",
        _ => null
    };

    private sealed record ApiError(string? Code, string? Message);

    private async Task<T?> PutAsync<T>(string baseUrl, string path, object request)
    {
        var response = await CreateClient().PutAsJsonAsync(BuildUrl(baseUrl, path), request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    private static string BuildUrl(string baseUrl, string path) => baseUrl.TrimEnd('/') + path;

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        if (_session.IsAuthenticated && !string.IsNullOrEmpty(_session.Login?.AccessToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _session.Login.AccessToken);
        }

        if (_session.TenantId is Guid tenantId)
        {
            client.DefaultRequestHeaders.Remove("x-tenant-id");
            client.DefaultRequestHeaders.Add("x-tenant-id", tenantId.ToString());
        }

        return client;
    }
}
