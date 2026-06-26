using Nexus.Services.Crm.Domain.Enums;
using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Repositories;
using Nexus.SharedKernel.Validation;

namespace Nexus.Services.Crm.Domain.Customers;

public sealed class Customer : FullAuditedAggregateRoot<Guid>
{
    private Customer()
    {
        Code = string.Empty;
        Name = string.Empty;
    }

    public Customer(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        CustomerType customerType,
        string? email,
        string? phone,
        Guid? creatorId,
        DateTimeOffset now)
    {
        Id = id;
        Code = NormalizeCode(code);
        Name = Check.Length(Check.NotNullOrWhiteSpace(name, nameof(name)), nameof(name), CustomerConsts.NameMaxLength);
        CustomerType = customerType;
        Email = NormalizeOptional(email, CustomerConsts.EmailMaxLength);
        Phone = NormalizeOptional(phone, CustomerConsts.PhoneMaxLength);
        Status = CustomerStatus.Active;
        SetCreationAudit(tenantId, creatorId, now);
    }

    public string Code { get; private set; }
    public string Name { get; private set; }
    public CustomerType CustomerType { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public CustomerStatus Status { get; private set; }
    public string? TaxCode { get; private set; }
    public string? Website { get; private set; }
    public string? Industry { get; private set; }
    public string? AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Country { get; private set; }
    public Guid? OwnerId { get; private set; }
    public string? Description { get; private set; }
    public LeadRating? Rating { get; private set; }
    public string? Source { get; private set; }

    public static string NormalizeCode(string code) =>
        Check.NotNullOrWhiteSpace(code, nameof(code)).Trim().ToUpperInvariant();

    public void UpdateProfile(
        string name,
        CustomerType customerType,
        string? email,
        string? phone,
        string? taxCode,
        string? website,
        string? industry,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? state,
        string? postalCode,
        string? country,
        Guid? ownerId,
        string? description,
        LeadRating? rating,
        string? source,
        CustomerStatus status,
        Guid? modifierId,
        DateTimeOffset now)
    {
        Name = Check.Length(Check.NotNullOrWhiteSpace(name, nameof(name)), nameof(name), CustomerConsts.NameMaxLength);
        CustomerType = customerType;
        Email = NormalizeOptional(email, CustomerConsts.EmailMaxLength);
        Phone = NormalizeOptional(phone, CustomerConsts.PhoneMaxLength);
        TaxCode = NormalizeOptional(taxCode, CustomerConsts.TaxCodeMaxLength);
        Website = NormalizeOptional(website, CustomerConsts.WebsiteMaxLength);
        Industry = NormalizeOptional(industry, CustomerConsts.IndustryMaxLength);
        AddressLine1 = NormalizeOptional(addressLine1, CustomerConsts.AddressMaxLength);
        AddressLine2 = NormalizeOptional(addressLine2, CustomerConsts.AddressMaxLength);
        City = NormalizeOptional(city, CustomerConsts.CityMaxLength);
        State = NormalizeOptional(state, CustomerConsts.StateMaxLength);
        PostalCode = NormalizeOptional(postalCode, CustomerConsts.PostalCodeMaxLength);
        Country = NormalizeOptional(country, CustomerConsts.CountryMaxLength);
        OwnerId = ownerId;
        Description = description?.Trim();
        Rating = rating;
        Source = NormalizeOptional(source, CustomerConsts.SourceMaxLength);
        Status = status;
        SetModificationAudit(modifierId, now);
    }

    private static string? NormalizeOptional(string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value) ? null : Check.Length(value.Trim(), nameof(value), maxLength);
}

public interface ICustomerRepository : IRepository<Customer, Guid>
{
    Task<Customer?> FindByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetListByTenantAsync(Guid tenantId, string? search, string? status, Guid? ownerId, int skipCount, int maxResultCount, string? sorting, CancellationToken cancellationToken = default);
    Task<long> GetCountByTenantAsync(Guid tenantId, string? search, string? status, Guid? ownerId, CancellationToken cancellationToken = default);
}
