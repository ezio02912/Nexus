using Nexus.ApiContracts.Dtos;

namespace Nexus.Services.Tenant.Contracts.Tenants;

public sealed class GetTenantsInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; init; }
}

public sealed class CreateTenantDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string RepresentativeName { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
    public string PlanCode { get; init; } = "FREE";
}

public sealed class CreateInternalTenantDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public required string RepresentativeName { get; init; }
    public required string ContactEmail { get; init; }
    public string PlanCode { get; init; } = "FREE";
    public IReadOnlyList<string> DefaultModules { get; init; } = [];
}

public sealed class ChangeTenantSubscriptionDto
{
    public required string PlanCode { get; init; }
}

public sealed class UpdateTenantProfileDto
{
    public required string Name { get; init; }
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public required string RepresentativeName { get; init; }
    public required string ContactEmail { get; init; }
}

public sealed class UpdateTenantSettingsDto
{
    public IReadOnlyDictionary<string, string> Settings { get; init; } = new Dictionary<string, string>();
}

public sealed class ChangeTenantModuleDto
{
    public required string ModuleCode { get; init; }
}

public sealed class TenantSubscriptionDto
{
    public string PlanCode { get; init; } = string.Empty;
    public string PlanName { get; init; } = string.Empty;
    public decimal MonthlyPrice { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}

public sealed class TenantDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string RepresentativeName { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public TenantSubscriptionDto? Subscription { get; init; }
    public IReadOnlyList<TenantModuleDto> Modules { get; init; } = [];
    public IReadOnlyDictionary<string, string> Settings { get; init; } = new Dictionary<string, string>();
    public string ConcurrencyStamp { get; init; } = string.Empty;
}

public sealed class TenantModuleDto
{
    public string ModuleCode { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
}
