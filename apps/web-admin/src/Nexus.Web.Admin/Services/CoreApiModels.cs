namespace Nexus.Web.Admin.Services;

public sealed record PagedResult<T>(long TotalCount, IReadOnlyList<T> Items);

// Response DTOs bound to BootstrapBlazor tables use settable properties (with a
// parameterless constructor) so the Table component can create header instances and bind columns.
public sealed record TenantDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string RepresentativeName { get; set; } = "";
    public string ContactEmail { get; set; } = "";
    public string Status { get; set; } = "";
    public TenantSubscriptionDto? Subscription { get; set; }
    public IReadOnlyList<TenantModuleDto>? Modules { get; set; }
    public IReadOnlyDictionary<string, string>? Settings { get; set; }
    public string? ConcurrencyStamp { get; set; }
}
public sealed record TenantSubscriptionDto
{
    public string PlanCode { get; set; } = "";
    public string PlanName { get; set; } = "";
    public decimal MonthlyPrice { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}
public sealed record TenantModuleDto(string ModuleCode, bool IsEnabled);
public sealed record CreateTenantRequest(string Code, string Name, string? Address = null, string? Phone = null, string RepresentativeName = "", string ContactEmail = "", string PlanCode = "FREE");
public sealed record ChangeTenantSubscriptionRequest(string PlanCode);
public sealed record UpdateTenantProfileRequest(string Name, string? Address, string? Phone, string RepresentativeName, string ContactEmail);
public sealed record ChangeTenantModuleRequest(string ModuleCode);
public sealed record UpdateTenantSettingsRequest(IReadOnlyDictionary<string, string> Settings);

public sealed record UserDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsActive { get; set; }
    public IReadOnlyCollection<string>? Roles { get; set; }
    public string? ConcurrencyStamp { get; set; }
}
public sealed record CreateUserRequest(Guid TenantId, string UserName, string Email, string Password, IReadOnlyCollection<string> Roles);
public sealed record LoginRequest(Guid TenantId, string UserName, string Password);
public sealed record LoginResult(Guid UserId, Guid TenantId, string AccessToken, DateTimeOffset ExpiresAt);

public sealed record RolePermissionDto(string RoleName, IReadOnlyCollection<string> Permissions);
public sealed record UpdateRolePermissionsRequest(IReadOnlyCollection<string> Permissions);

public sealed record AuditLogEntry
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string ServiceName { get; set; } = "";
    public string EntityName { get; set; } = "";
    public string? EntityId { get; set; }
    public int Action { get; set; }
    public string? Summary { get; set; }
    public string? CorrelationId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}
public sealed record CreateAuditLogRequest(Guid? TenantId, Guid? UserId, string ServiceName, string EntityName, string? EntityId, int Action, string? Summary, string? CorrelationId);

public sealed record FileRecord
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long Size { get; set; }
    public string StoragePath { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public IReadOnlyList<FileLinkRecord>? Links { get; set; }
}
public sealed record FileLinkRecord(Guid Id, Guid FileId, string Module, string EntityType, string EntityId, DateTimeOffset CreatedAt);
public sealed record CreateFileRequest(Guid? TenantId, string FileName, string ContentType, long Size, string StoragePath);
public sealed record CreateFileLinkRequest(Guid FileId, string Module, string EntityType, string EntityId);

public sealed record NextNumberRequest(Guid? TenantId, string Module, string DocumentType, string Prefix, int Padding, string? Period);
public sealed record NextNumberResult(string SequenceKey, string Number, long Value);
public sealed record NumberSequenceDto
{
    public string Key { get; set; } = "";
    public long CurrentValue { get; set; }
}

public sealed record WorkflowDefinitionRecord
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public IReadOnlyList<string> Steps { get; set; } = [];
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
public sealed record CreateWorkflowDefinitionRequest(Guid? TenantId, string Code, string Name, IReadOnlyCollection<string> Steps);
public sealed record CreateWorkflowInstanceRequest(Guid? TenantId, Guid WorkflowDefinitionId, string SourceModule, string SourceType, string SourceId);
public sealed record WorkflowActionRequest(Guid UserId, string? Comment);
public sealed record WorkflowInstanceRecord(Guid Id, Guid? TenantId, Guid WorkflowDefinitionId, string SourceModule, string SourceType, string SourceId, string Status, IReadOnlyList<WorkflowActionRecord> Actions, DateTimeOffset CreatedAt);
public sealed record WorkflowActionRecord(Guid Id, Guid UserId, string Action, string? Comment, DateTimeOffset CreatedAt);

public sealed record MasterDataCategoryDto(string Code, string Label);

public sealed record LookupItemDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = "";
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public sealed record CreateLookupItemRequest(string Category, string Code, string Name, int SortOrder, bool IsActive);
public sealed record UpdateLookupItemRequest(string Code, string Name, int SortOrder, bool IsActive);

public sealed record SubscriptionPlanDto
{
    public string PlanCode { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal MonthlyPrice { get; set; }
    public IReadOnlyList<string>? Modules { get; set; }
    public int MaxUsers { get; set; }
    public int StorageGb { get; set; }
    public int TierOrder { get; set; }
}

public sealed record PlatformDashboardDto
{
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int SuspendedTenants { get; set; }
    public int NewTenantsLast7Days { get; set; }
    public int NewTenantsLast30Days { get; set; }
    public int NewUsersLast7Days { get; set; }
    public int NewUsersLast30Days { get; set; }
    public int ActiveSubscriptions { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
    public IReadOnlyDictionary<string, int>? ActiveSubscriptionsByPlan { get; set; }
    public IReadOnlyList<PlatformTimeSeriesPointDto>? TenantGrowthSeries { get; set; }
    public IReadOnlyList<PlatformTimeSeriesPointDto>? UserGrowthSeries { get; set; }
    public IReadOnlyList<PlatformRevenuePointDto>? RevenueLast6Months { get; set; }
    public IReadOnlyList<PlatformRecentTenantDto>? RecentTenants { get; set; }
}

public sealed record PlatformTimeSeriesPointDto(DateOnly Date, int Count);
public sealed record PlatformRevenuePointDto(string Month, decimal Amount);
public sealed class PlatformRecentTenantDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? PlanCode { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed record SubscriptionPaymentDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string PlanCode { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "";
    public string? MockReference { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
}
