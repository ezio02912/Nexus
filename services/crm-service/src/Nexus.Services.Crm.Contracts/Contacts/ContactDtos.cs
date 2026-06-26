using Nexus.ApiContracts.Dtos;

namespace Nexus.Services.Crm.Contracts.Contacts;

public sealed class GetContactsInput : PagedAndSortedResultRequestDto
{
    public Guid? CustomerId { get; init; }
    public string? Search { get; init; }
}

public sealed class CreateContactDto
{
    public required Guid CustomerId { get; init; }
    public required string FullName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Mobile { get; init; }
    public string? Position { get; init; }
    public string? Department { get; init; }
    public bool IsPrimary { get; init; }
    public bool IsDecisionMaker { get; init; }
    public Guid? OwnerId { get; init; }
}

public sealed class UpdateContactDto
{
    public required Guid CustomerId { get; init; }
    public required string FullName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Mobile { get; init; }
    public string? Position { get; init; }
    public string? Department { get; init; }
    public bool IsPrimary { get; init; }
    public bool IsDecisionMaker { get; init; }
    public string? LinkedInUrl { get; init; }
    public string? Notes { get; init; }
    public Guid? OwnerId { get; init; }
}

public sealed class ContactDto
{
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    public Guid CustomerId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Mobile { get; init; }
    public string? Position { get; init; }
    public string? Department { get; init; }
    public bool IsPrimary { get; init; }
    public bool IsDecisionMaker { get; init; }
    public string? LinkedInUrl { get; init; }
    public string? Notes { get; init; }
    public Guid? OwnerId { get; init; }
    public DateTimeOffset CreationTime { get; init; }
    public Guid? CreatorId { get; init; }
    public DateTimeOffset? LastModificationTime { get; init; }
    public Guid? LastModifierId { get; init; }
    public string ConcurrencyStamp { get; init; } = string.Empty;
}
