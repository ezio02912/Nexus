using Nexus.Services.Crm.Domain.Enums;
using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Repositories;
using Nexus.SharedKernel.Validation;

namespace Nexus.Services.Crm.Domain.Leads;

public static class LeadConsts
{
    public const int FullNameMaxLength = 256;
    public const int CompanyNameMaxLength = 256;
    public const int EmailMaxLength = 256;
    public const int PhoneMaxLength = 64;
    public const int SourceMaxLength = 128;
    public const int TitleMaxLength = 128;
    public const int AddressMaxLength = 256;
    public const int CityMaxLength = 128;
    public const int CountryMaxLength = 64;
}

public sealed class Lead : FullAuditedAggregateRoot<Guid>
{
    private Lead()
    {
        FullName = string.Empty;
    }

    public Lead(
        Guid id,
        Guid tenantId,
        string fullName,
        string? companyName,
        string? title,
        string? email,
        string? phone,
        string? source,
        Guid? ownerId,
        Guid? creatorId,
        DateTimeOffset now)
    {
        Id = id;
        FullName = Check.Length(Check.NotNullOrWhiteSpace(fullName, nameof(fullName)), nameof(fullName), LeadConsts.FullNameMaxLength);
        CompanyName = NormalizeOptional(companyName, LeadConsts.CompanyNameMaxLength);
        Title = NormalizeOptional(title, LeadConsts.TitleMaxLength);
        Email = NormalizeOptional(email, LeadConsts.EmailMaxLength);
        Phone = NormalizeOptional(phone, LeadConsts.PhoneMaxLength);
        Source = NormalizeOptional(source, LeadConsts.SourceMaxLength);
        OwnerId = ownerId;
        Status = LeadStatus.New;
        LeadScore = 0;
        SetCreationAudit(tenantId, creatorId, now);
    }

    public string FullName { get; private set; }
    public string? CompanyName { get; private set; }
    public string? Title { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Source { get; private set; }
    public LeadStatus Status { get; private set; }
    public int LeadScore { get; private set; }
    public LeadRating? Rating { get; private set; }
    public Guid? OwnerId { get; private set; }
    public DateTimeOffset? AssignedAt { get; private set; }
    public Guid? ConvertedCustomerId { get; private set; }
    public Guid? ConvertedOpportunityId { get; private set; }
    public DateTimeOffset? ConvertedAt { get; private set; }
    public string? LostReason { get; private set; }
    public string? Description { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? Country { get; private set; }

    public void Update(
        string fullName,
        string? companyName,
        string? title,
        string? email,
        string? phone,
        string? source,
        int leadScore,
        LeadRating? rating,
        LeadStatus status,
        Guid? ownerId,
        string? description,
        string? address,
        string? city,
        string? country,
        string? lostReason,
        Guid? modifierId,
        DateTimeOffset now)
    {
        FullName = Check.Length(Check.NotNullOrWhiteSpace(fullName, nameof(fullName)), nameof(fullName), LeadConsts.FullNameMaxLength);
        CompanyName = NormalizeOptional(companyName, LeadConsts.CompanyNameMaxLength);
        Title = NormalizeOptional(title, LeadConsts.TitleMaxLength);
        Email = NormalizeOptional(email, LeadConsts.EmailMaxLength);
        Phone = NormalizeOptional(phone, LeadConsts.PhoneMaxLength);
        Source = NormalizeOptional(source, LeadConsts.SourceMaxLength);
        LeadScore = Math.Clamp(leadScore, 0, 100);
        Rating = rating;
        Status = status;
        OwnerId = ownerId;
        Description = description?.Trim();
        Address = NormalizeOptional(address, LeadConsts.AddressMaxLength);
        City = NormalizeOptional(city, LeadConsts.CityMaxLength);
        Country = NormalizeOptional(country, LeadConsts.CountryMaxLength);
        LostReason = lostReason?.Trim();
        SetModificationAudit(modifierId, now);
    }

    public void MarkConverted(Guid customerId, Guid opportunityId, Guid? modifierId, DateTimeOffset now)
    {
        if (Status == LeadStatus.Converted)
        {
            throw new InvalidOperationException("Lead is already converted.");
        }

        Status = LeadStatus.Converted;
        ConvertedCustomerId = customerId;
        ConvertedOpportunityId = opportunityId;
        ConvertedAt = now;
        SetModificationAudit(modifierId, now);
    }

    private static string? NormalizeOptional(string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value) ? null : Check.Length(value.Trim(), nameof(value), maxLength);
}

public interface ILeadRepository : IRepository<Lead, Guid>
{
    Task<IReadOnlyList<Lead>> GetListByTenantAsync(Guid tenantId, string? search, string? status, Guid? ownerId, int skipCount, int maxResultCount, CancellationToken cancellationToken = default);
    Task<long> GetCountByTenantAsync(Guid tenantId, string? search, string? status, Guid? ownerId, CancellationToken cancellationToken = default);
}
