namespace Nexus.Web.Tenant.Services;

public sealed record PagedResult<T>(long TotalCount, IReadOnlyList<T> Items);
public sealed record TenantDto(Guid Id, string Code, string Name, string Status, IReadOnlyList<TenantModuleDto>? Modules, IReadOnlyDictionary<string, string>? Settings, string? ConcurrencyStamp);
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

public sealed record CustomerRecord(Guid Id, Guid TenantId, string Code, string Name, string? Email, string? Phone, string Status, DateTimeOffset CreatedAt);
public sealed record CreateCustomerRequest(Guid TenantId, string Code, string Name, string? Email, string? Phone);
public sealed record LeadRecord(Guid Id, Guid TenantId, string FullName, string? CompanyName, string? Email, string? Phone, string? Source, string Status, DateTimeOffset CreatedAt);
public sealed record CreateLeadRequest(Guid TenantId, string FullName, string? CompanyName, string? Email, string? Phone, string? Source);
public sealed record OpportunityRecord(Guid Id, Guid TenantId, Guid? CustomerId, string Name, string Stage, decimal Amount, DateOnly? ExpectedCloseDate, DateTimeOffset CreatedAt);
public sealed record CreateOpportunityRequest(Guid TenantId, Guid? CustomerId, string Name, decimal Amount, DateOnly? ExpectedCloseDate);
public sealed record QuotationRecord(Guid Id, Guid TenantId, Guid CustomerId, string QuotationNo, decimal TotalAmount, string Status, DateTimeOffset CreatedAt, DateTimeOffset? ApprovedAt);
public sealed record CreateQuotationRequest(Guid TenantId, Guid CustomerId, string QuotationNo, decimal TotalAmount);
public sealed record ContractRecord(Guid Id, Guid TenantId, Guid CustomerId, string ContractNo, string Title, string Status, DateTimeOffset? SignedAt, DateTimeOffset CreatedAt);
public sealed record CreateContractRequest(Guid TenantId, Guid CustomerId, string ContractNo, string Title);
public sealed record SalesOrderRecord(Guid Id, Guid TenantId, Guid CustomerId, string OrderNo, string Status, decimal TotalAmount, IReadOnlyList<SalesOrderLineRecord>? Lines, DateTimeOffset CreatedAt, DateTimeOffset? ApprovedAt, DateTimeOffset? CompletedAt);
public sealed record SalesOrderLineRecord(Guid Id, string ProductCode, string Description, decimal Quantity, decimal UnitPrice, decimal LineAmount);
public sealed record CreateSalesOrderRequest(Guid TenantId, Guid CustomerId, string OrderNo, IReadOnlyCollection<CreateSalesOrderLineRequest> Lines);
public sealed record CreateSalesOrderLineRequest(string ProductCode, string Description, decimal Quantity, decimal UnitPrice);
