using Nexus.ApiContracts.Dtos;
using Nexus.Services.Crm.Domain.Enums;

namespace Nexus.Services.Crm.Contracts.Customers;

public sealed class GetCustomersInput : PagedAndSortedResultRequestDto
{
    public string? Search { get; init; }
    public CustomerStatus? Status { get; init; }
    public Guid? OwnerId { get; init; }
}

public sealed class CreateCustomerDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public CustomerType CustomerType { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Industry { get; init; }
    public string? City { get; init; }
    public string? Source { get; init; }
}

public sealed class UpdateCustomerDto
{
    public required string Name { get; init; }
    public CustomerType CustomerType { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? TaxCode { get; init; }
    public string? Website { get; init; }
    public string? Industry { get; init; }
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public Guid? OwnerId { get; init; }
    public string? Description { get; init; }
    public LeadRating? Rating { get; init; }
    public string? Source { get; init; }
    public CustomerStatus Status { get; init; }
}

public sealed class CustomerDto
{
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public CustomerType CustomerType { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public CustomerStatus Status { get; init; }
    public string? TaxCode { get; init; }
    public string? Website { get; init; }
    public string? Industry { get; init; }
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public Guid? OwnerId { get; init; }
    public string? Description { get; init; }
    public LeadRating? Rating { get; init; }
    public string? Source { get; init; }
    public DateTimeOffset CreationTime { get; init; }
    public Guid? CreatorId { get; init; }
    public DateTimeOffset? LastModificationTime { get; init; }
    public Guid? LastModifierId { get; init; }
    public string ConcurrencyStamp { get; init; } = string.Empty;
}
