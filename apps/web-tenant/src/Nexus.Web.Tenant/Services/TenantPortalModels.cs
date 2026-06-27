namespace Nexus.Web.Tenant.Services;

public sealed record PagedResult<T>(long TotalCount, IReadOnlyList<T> Items);
public sealed record TenantSubscriptionDto(string PlanCode, string PlanName, decimal MonthlyPrice, DateTimeOffset? ExpiresAt);
public sealed record TenantDto(
    Guid Id,
    string Code,
    string Name,
    string Status,
    IReadOnlyList<TenantModuleDto>? Modules,
    IReadOnlyDictionary<string, string>? Settings,
    string? ConcurrencyStamp,
    TenantSubscriptionDto? Subscription = null);
public sealed record SubscriptionPlanDto(string PlanCode, string Name, decimal MonthlyPrice, IReadOnlyList<string>? Modules, int MaxUsers, int StorageGb, int TierOrder);
public sealed record CreateCheckoutRequest(string TargetPlanCode);
public sealed record CheckoutSessionDto(Guid CheckoutId, string TargetPlanCode, string TargetPlanName, decimal Amount, string MockCardNumber);
public sealed record SubscriptionPaymentDto(Guid Id, Guid TenantId, string PlanCode, decimal Amount, string Status, string? MockReference, DateTimeOffset CreatedAt, DateTimeOffset? PaidAt);
public sealed record TenantModuleDto(string ModuleCode, bool IsEnabled);
public sealed record LoginRequest(Guid TenantId, string UserName, string Password);
public sealed record LoginEmailRequest(string Email, string Password);
public sealed record LoginResult(Guid UserId, Guid TenantId, string AccessToken, DateTimeOffset ExpiresAt, string? RefreshToken = null, string? TenantCode = null, string? UserName = null, string? Email = null);
public sealed record GoogleAuthRequest(string? IdToken, string? AccessToken = null);
public sealed record GoogleAuthResult(string Status, string? OnboardingToken, string? Email, string? DisplayName, LoginResult? Login);
public sealed record PreviewTenantCodeRequest(string CompanyName);
public sealed record PreviewTenantCodeResult(string SuggestedCode, bool Available);
public sealed record CompleteOnboardingRequest(
    string OnboardingToken,
    string CompanyName,
    string Code,
    string RepresentativeName,
    string? Address,
    string? Phone,
    string? UserName,
    string? Password);
public sealed record CompleteOnboardingResult(Guid TenantId, string TenantCode, string TenantName, LoginResult Login);
public sealed record UserDto(Guid Id, Guid TenantId, string UserName, string Email, bool IsActive, IReadOnlyCollection<string>? Roles, string? ConcurrencyStamp);
public sealed record CreateUserRequest(Guid TenantId, string UserName, string Email, string Password, IReadOnlyCollection<string> Roles);
public sealed record RolePermissionDto(string RoleName, IReadOnlyCollection<string> Permissions);
public sealed record UpdateRolePermissionsRequest(IReadOnlyCollection<string> Permissions);

public sealed record SalesOrderRecord(Guid Id, Guid TenantId, Guid CustomerId, string OrderNo, string? SourceType, Guid? SourceId, string? SourceNo, string Status, string InventoryReservationStatus, string DeliveryStatus, decimal Subtotal, decimal DiscountAmount, decimal TaxAmount, decimal TotalAmount, IReadOnlyList<SalesOrderLineRecord>? Lines, DateTimeOffset CreatedAt, DateTimeOffset? ApprovedAt, DateTimeOffset? ReservedAt, DateTimeOffset? DeliveredAt, DateTimeOffset? CompletedAt);
public sealed record SalesOrderLineRecord(Guid Id, string ProductCode, string Description, decimal Quantity, decimal UnitPrice, decimal DiscountPercent, decimal DiscountAmount, decimal TaxPercent, decimal TaxAmount, decimal Subtotal, decimal LineAmount);
public sealed record CreateSalesOrderRequest(Guid TenantId, Guid CustomerId, string OrderNo, string? SourceType, Guid? SourceId, string? SourceNo, IReadOnlyCollection<CreateSalesOrderLineRequest> Lines);
public sealed record CreateSalesOrderLineRequest(string ProductCode, string Description, decimal Quantity, decimal UnitPrice, decimal DiscountPercent, decimal TaxPercent);

public sealed record StockBalanceRecord(Guid Id, Guid TenantId, string WarehouseCode, string ProductCode, string ProductName, decimal OnHandQuantity, decimal ReservedQuantity, decimal AvailableQuantity, DateTimeOffset UpdatedAt);
public sealed record ImportStockRequest(Guid TenantId, string WarehouseCode, string ProductCode, string ProductName, decimal Quantity, string? SourceType, Guid? SourceId, string? SourceNo);
public sealed record InventoryProductRecord(Guid Id, Guid TenantId, string ProductCode, string ProductName, string Unit, string Category, decimal Price, decimal TaxPercent, bool IsActive, DateTimeOffset UpdatedAt);
public sealed record UpsertInventoryProductRequest(Guid TenantId, string ProductCode, string ProductName, string Unit, string? Category, decimal Price, decimal TaxPercent, bool IsActive);
public sealed record WarehouseRecord(Guid Id, Guid TenantId, string WarehouseCode, string Name, string Location, bool IsActive, DateTimeOffset UpdatedAt);
public sealed record UpsertWarehouseRequest(Guid TenantId, string WarehouseCode, string Name, string? Location, bool IsActive);
