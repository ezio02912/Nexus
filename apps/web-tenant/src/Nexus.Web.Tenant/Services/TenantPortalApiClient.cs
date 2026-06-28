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
    public Task<SalesOrderRecord?> UnapproveSalesOrderAsync(Guid id) => PostAsync<SalesOrderRecord>(_options.Sales, $"/api/sales/orders/{id}/unapprove", new { });
    public Task DeleteSalesOrderAsync(Guid id) => DeleteAsync(_options.Sales, $"/api/sales/orders/{id}");

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

    public Task<StockTransferRecord?> TransferStockAsync(TransferStockRequest request) =>
        PostAsync<StockTransferRecord>(_options.Inventory, "/api/inventory/transfers", request);

    public Task<IReadOnlyList<StockTransferRecord>?> GetStockTransfersAsync(Guid tenantId, string? search = null)
    {
        var path = $"/api/inventory/transfers?tenantId={tenantId}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            path += $"&search={Uri.EscapeDataString(search)}";
        }

        return GetAsync<IReadOnlyList<StockTransferRecord>>(_options.Inventory, path);
    }

    public Task<StockTransferRecord?> GetStockTransferAsync(Guid tenantId, Guid id) =>
        GetAsync<StockTransferRecord>(_options.Inventory, $"/api/inventory/transfers/{id}?tenantId={tenantId}");

    public Task<IReadOnlyList<StockMovementRecord>?> GetStockMovementsAsync(Guid tenantId, string? productCode = null, string? warehouseCode = null)
    {
        var path = $"/api/inventory/movements?tenantId={tenantId}";
        if (!string.IsNullOrWhiteSpace(productCode))
        {
            path += $"&productCode={Uri.EscapeDataString(productCode)}";
        }

        if (!string.IsNullOrWhiteSpace(warehouseCode))
        {
            path += $"&warehouseCode={Uri.EscapeDataString(warehouseCode)}";
        }

        return GetAsync<IReadOnlyList<StockMovementRecord>>(_options.Inventory, path);
    }

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

    public Task<PurchaseOrderRecord?> UnapprovePurchaseOrderAsync(Guid id) =>
        PostAsync<PurchaseOrderRecord>(_options.Purchase, $"/api/purchase/orders/{id}/unapprove", new { });

    public Task DeletePurchaseOrderAsync(Guid id) => DeleteAsync(_options.Purchase, $"/api/purchase/orders/{id}");

    public Task<IReadOnlyList<GoodsReceiptRecord>?> GetGoodsReceiptsAsync(Guid tenantId) =>
        GetAsync<IReadOnlyList<GoodsReceiptRecord>>(_options.Purchase, $"/api/purchase/goods-receipts?tenantId={tenantId}");

    public Task<IReadOnlyList<HrmEmployeeRecord>?> GetHrmEmployeesAsync(Guid tenantId, string? search = null) =>
        GetListAsync<HrmEmployeeRecord>(_options.Hrm, "/api/hrm/employees", tenantId, search);

    public Task<IReadOnlyList<HrmDepartmentRecord>?> GetHrmDepartmentsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<HrmDepartmentRecord>(_options.Hrm, "/api/hrm/departments", tenantId, search);

    public Task<IReadOnlyList<HrmPositionRecord>?> GetHrmPositionsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<HrmPositionRecord>(_options.Hrm, "/api/hrm/positions", tenantId, search);

    public Task<IReadOnlyList<HrmContractRecord>?> GetHrmContractsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<HrmContractRecord>(_options.Hrm, "/api/hrm/contracts", tenantId, search);

    public Task<IReadOnlyList<HrmEmployeeAllowanceRecord>?> GetHrmEmployeeAllowancesAsync(Guid tenantId, string? search = null) =>
        GetListAsync<HrmEmployeeAllowanceRecord>(_options.Hrm, "/api/hrm/allowances", tenantId, search);

    public Task<IReadOnlyList<HrmEmployeeBenefitRecord>?> GetHrmEmployeeBenefitsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<HrmEmployeeBenefitRecord>(_options.Hrm, "/api/hrm/benefits", tenantId, search);

    public Task<IReadOnlyList<HrmEmployeeDocumentRecord>?> GetHrmEmployeeDocumentsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<HrmEmployeeDocumentRecord>(_options.Hrm, "/api/hrm/documents", tenantId, search);

    public Task<IReadOnlyList<HrmRequisitionRecord>?> GetHrmRequisitionsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<HrmRequisitionRecord>(_options.Hrm, "/api/hrm/requisitions", tenantId, search);

    public Task<IReadOnlyList<HrmCandidateRecord>?> GetHrmCandidatesAsync(Guid tenantId, string? search = null) =>
        GetListAsync<HrmCandidateRecord>(_options.Hrm, "/api/hrm/candidates", tenantId, search);

    public Task<IReadOnlyList<HrmApplicationRecord>?> GetHrmApplicationsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<HrmApplicationRecord>(_options.Hrm, "/api/hrm/applications", tenantId, search);

    public Task<IReadOnlyList<HrmOfferRecord>?> GetHrmOffersAsync(Guid tenantId, string? search = null) =>
        GetListAsync<HrmOfferRecord>(_options.Hrm, "/api/hrm/offers", tenantId, search);

    public Task<HrmEmployeeRecord?> AcceptHrmOfferAsync(Guid offerId) =>
        PostAsync<HrmEmployeeRecord>(_options.Hrm, $"/api/hrm/offers/{offerId}/accept", new { });

    public Task<HrmEmployeeRecord?> UpsertHrmEmployeeAsync(HrmEmployeeRecord request) =>
        PostAsync<HrmEmployeeRecord>(_options.Hrm, "/api/hrm/employees", request);

    public Task<HrmDepartmentRecord?> UpsertHrmDepartmentAsync(HrmDepartmentRecord request) =>
        PostAsync<HrmDepartmentRecord>(_options.Hrm, "/api/hrm/departments", request);

    public Task<HrmPositionRecord?> UpsertHrmPositionAsync(HrmPositionRecord request) =>
        PostAsync<HrmPositionRecord>(_options.Hrm, "/api/hrm/positions", request);

    public Task<HrmContractRecord?> UpsertHrmContractAsync(HrmContractRecord request) =>
        PostAsync<HrmContractRecord>(_options.Hrm, "/api/hrm/contracts", request);

    public Task<HrmEmployeeAllowanceRecord?> UpsertHrmEmployeeAllowanceAsync(HrmEmployeeAllowanceRecord request) =>
        PostAsync<HrmEmployeeAllowanceRecord>(_options.Hrm, "/api/hrm/allowances", request);

    public Task<HrmEmployeeBenefitRecord?> UpsertHrmEmployeeBenefitAsync(HrmEmployeeBenefitRecord request) =>
        PostAsync<HrmEmployeeBenefitRecord>(_options.Hrm, "/api/hrm/benefits", request);

    public Task<HrmEmployeeDocumentRecord?> UpsertHrmEmployeeDocumentAsync(HrmEmployeeDocumentRecord request) =>
        PostAsync<HrmEmployeeDocumentRecord>(_options.Hrm, "/api/hrm/documents", request);

    public Task<HrmRequisitionRecord?> UpsertHrmRequisitionAsync(HrmRequisitionRecord request) =>
        PostAsync<HrmRequisitionRecord>(_options.Hrm, "/api/hrm/requisitions", request);

    public Task<HrmCandidateRecord?> UpsertHrmCandidateAsync(HrmCandidateRecord request) =>
        PostAsync<HrmCandidateRecord>(_options.Hrm, "/api/hrm/candidates", request);

    public Task DeleteHrmEntityAsync(string endpoint, Guid tenantId, Guid id) =>
        DeleteAsync(_options.Hrm, $"/api/hrm/{endpoint}/{id}?tenantId={tenantId}");

    public Task<IReadOnlyList<AttendanceCalendarRecord>?> GetAttendanceCalendarsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<AttendanceCalendarRecord>(_options.Attendance, "/api/attendance/work-calendars", tenantId, search);

    public Task<IReadOnlyList<AttendanceShiftRecord>?> GetAttendanceShiftsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<AttendanceShiftRecord>(_options.Attendance, "/api/attendance/shifts", tenantId, search);

    public Task<IReadOnlyList<AttendanceRecordItem>?> GetAttendanceRecordsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<AttendanceRecordItem>(_options.Attendance, "/api/attendance/records", tenantId, search);

    public Task<IReadOnlyList<LeaveRequestRecord>?> GetLeaveRequestsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<LeaveRequestRecord>(_options.Attendance, "/api/attendance/leave-requests", tenantId, search);

    public Task<IReadOnlyList<LeaveTypeRecord>?> GetLeaveTypesAsync(Guid tenantId, string? search = null) =>
        GetListAsync<LeaveTypeRecord>(_options.Attendance, "/api/attendance/leave-types", tenantId, search);

    public Task<IReadOnlyList<HolidayRecord>?> GetHolidaysAsync(Guid tenantId, string? search = null) =>
        GetListAsync<HolidayRecord>(_options.Attendance, "/api/attendance/holidays", tenantId, search);

    public Task<IReadOnlyList<OvertimeRequestRecord>?> GetOvertimeRequestsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<OvertimeRequestRecord>(_options.Attendance, "/api/attendance/overtime-requests", tenantId, search);

    public Task<object?> SetupAttendanceVietnamDefaultsAsync(Guid tenantId) =>
        PostAsync<object>(_options.Attendance, "/api/attendance/setup-vn-defaults", new TenantScopedRequest(tenantId));

    public Task<LeaveRequestRecord?> ApproveLeaveRequestAsync(Guid id) =>
        PostAsync<LeaveRequestRecord>(_options.Attendance, $"/api/attendance/leave-requests/{id}/approve", new ApprovalRequest(_session.Login?.UserId));

    public Task<OvertimeRequestRecord?> ApproveOvertimeRequestAsync(Guid id) =>
        PostAsync<OvertimeRequestRecord>(_options.Attendance, $"/api/attendance/overtime-requests/{id}/approve", new ApprovalRequest(_session.Login?.UserId));

    public Task<AttendanceCalendarRecord?> UpsertAttendanceCalendarAsync(AttendanceCalendarRecord request) =>
        PostAsync<AttendanceCalendarRecord>(_options.Attendance, "/api/attendance/work-calendars", request);

    public Task<AttendanceShiftRecord?> UpsertAttendanceShiftAsync(AttendanceShiftRecord request) =>
        PostAsync<AttendanceShiftRecord>(_options.Attendance, "/api/attendance/shifts", request);

    public Task<AttendanceRecordItem?> UpsertAttendanceRecordAsync(AttendanceRecordItem request) =>
        PostAsync<AttendanceRecordItem>(_options.Attendance, "/api/attendance/records", request);

    public Task<LeaveRequestRecord?> UpsertLeaveRequestAsync(LeaveRequestRecord request) =>
        PostAsync<LeaveRequestRecord>(_options.Attendance, "/api/attendance/leave-requests", request);

    public Task<LeaveTypeRecord?> UpsertLeaveTypeAsync(LeaveTypeRecord request) =>
        PostAsync<LeaveTypeRecord>(_options.Attendance, "/api/attendance/leave-types", request);

    public Task<HolidayRecord?> UpsertHolidayAsync(HolidayRecord request) =>
        PostAsync<HolidayRecord>(_options.Attendance, "/api/attendance/holidays", request);

    public Task<OvertimeRequestRecord?> UpsertOvertimeRequestAsync(OvertimeRequestRecord request) =>
        PostAsync<OvertimeRequestRecord>(_options.Attendance, "/api/attendance/overtime-requests", request);

    public Task DeleteAttendanceEntityAsync(string endpoint, Guid tenantId, Guid id) =>
        DeleteAsync(_options.Attendance, $"/api/attendance/{endpoint}/{id}?tenantId={tenantId}");

    public Task<IReadOnlyList<PayrollPolicyRecord>?> GetPayrollPoliciesAsync(Guid tenantId, string? search = null) =>
        GetListAsync<PayrollPolicyRecord>(_options.Payroll, "/api/payroll/policies", tenantId, search);

    public Task<IReadOnlyList<SalaryComponentRecord>?> GetSalaryComponentsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<SalaryComponentRecord>(_options.Payroll, "/api/payroll/components", tenantId, search);

    public Task<IReadOnlyList<PayrollPeriodRecord>?> GetPayrollPeriodsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<PayrollPeriodRecord>(_options.Payroll, "/api/payroll/periods", tenantId, search);

    public Task<IReadOnlyList<PayrollRunRecord>?> GetPayrollRunsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<PayrollRunRecord>(_options.Payroll, "/api/payroll/runs", tenantId, search);

    public Task<IReadOnlyList<PayslipRecord>?> GetPayslipsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<PayslipRecord>(_options.Payroll, "/api/payroll/payslips", tenantId, search);

    public Task<IReadOnlyList<PayrollPaymentRecord>?> GetPayrollPaymentsAsync(Guid tenantId, string? search = null) =>
        GetListAsync<PayrollPaymentRecord>(_options.Payroll, "/api/payroll/payments", tenantId, search);

    public Task<object?> SetupPayrollVietnamDefaultsAsync(Guid tenantId) =>
        PostAsync<object>(_options.Payroll, "/api/payroll/setup-vn-defaults", new TenantScopedRequest(tenantId));

    public Task<PayrollRunRecord?> CalculatePayrollRunAsync(Guid id) =>
        PostAsync<PayrollRunRecord>(_options.Payroll, $"/api/payroll/runs/{id}/calculate", new { });

    public Task<PayrollRunRecord?> ApprovePayrollRunAsync(Guid id) =>
        PostAsync<PayrollRunRecord>(_options.Payroll, $"/api/payroll/runs/{id}/approve", new ApprovalRequest(_session.Login?.UserId));

    public Task<PayrollRunRecord?> PayPayrollRunAsync(Guid id) =>
        PostAsync<PayrollRunRecord>(_options.Payroll, $"/api/payroll/runs/{id}/pay", new { });

    public Task<object?> PublishPayslipsAsync(Guid id) =>
        PostAsync<object>(_options.Payroll, $"/api/payroll/runs/{id}/publish-payslips", new { });

    public Task<PayrollPolicyRecord?> UpsertPayrollPolicyAsync(PayrollPolicyRecord request) =>
        PostAsync<PayrollPolicyRecord>(_options.Payroll, "/api/payroll/policies", request);

    public Task<SalaryComponentRecord?> UpsertSalaryComponentAsync(SalaryComponentRecord request) =>
        PostAsync<SalaryComponentRecord>(_options.Payroll, "/api/payroll/components", request);

    public Task<PayrollPeriodRecord?> UpsertPayrollPeriodAsync(PayrollPeriodRecord request) =>
        PostAsync<PayrollPeriodRecord>(_options.Payroll, "/api/payroll/periods", request);

    public Task<PayrollRunRecord?> UpsertPayrollRunAsync(PayrollRunRecord request) =>
        PostAsync<PayrollRunRecord>(_options.Payroll, "/api/payroll/runs", request);

    public Task<PayslipRecord?> UpsertPayslipAsync(PayslipRecord request) =>
        PostAsync<PayslipRecord>(_options.Payroll, "/api/payroll/payslips", request);

    public Task<PayrollPaymentRecord?> UpsertPayrollPaymentAsync(PayrollPaymentRecord request) =>
        PostAsync<PayrollPaymentRecord>(_options.Payroll, "/api/payroll/payments", request);

    public Task DeletePayrollEntityAsync(string endpoint, Guid tenantId, Guid id) =>
        DeleteAsync(_options.Payroll, $"/api/payroll/{endpoint}/{id}?tenantId={tenantId}");

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

    private Task<IReadOnlyList<T>?> GetListAsync<T>(string baseUrl, string path, Guid tenantId, string? search)
    {
        var query = $"{path}?tenantId={tenantId}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            query += $"&search={Uri.EscapeDataString(search)}";
        }

        return GetAsync<IReadOnlyList<T>>(baseUrl, query);
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

    private async Task DeleteAsync(string baseUrl, string path)
    {
        var response = await CreateClient().DeleteAsync(BuildUrl(baseUrl, path));
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await ReadErrorMessageAsync(response));
        }
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
