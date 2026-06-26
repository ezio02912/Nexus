using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Repositories;
using Nexus.SharedKernel.Validation;

namespace Nexus.Services.Crm.Domain.Contacts;

public static class ContactConsts
{
    public const int FullNameMaxLength = 256;
    public const int EmailMaxLength = 256;
    public const int PhoneMaxLength = 64;
    public const int MobileMaxLength = 64;
    public const int PositionMaxLength = 128;
    public const int DepartmentMaxLength = 128;
    public const int LinkedInUrlMaxLength = 512;
}

public sealed class Contact : FullAuditedAggregateRoot<Guid>
{
    private Contact()
    {
        FullName = string.Empty;
    }

    public Contact(
        Guid id,
        Guid tenantId,
        Guid customerId,
        string fullName,
        string? email,
        string? phone,
        string? mobile,
        string? position,
        string? department,
        bool isPrimary,
        bool isDecisionMaker,
        Guid? ownerId,
        Guid? creatorId,
        DateTimeOffset now)
    {
        Id = id;
        CustomerId = customerId;
        FullName = Check.Length(Check.NotNullOrWhiteSpace(fullName, nameof(fullName)), nameof(fullName), ContactConsts.FullNameMaxLength);
        Email = NormalizeOptional(email, ContactConsts.EmailMaxLength);
        Phone = NormalizeOptional(phone, ContactConsts.PhoneMaxLength);
        Mobile = NormalizeOptional(mobile, ContactConsts.MobileMaxLength);
        Position = NormalizeOptional(position, ContactConsts.PositionMaxLength);
        Department = NormalizeOptional(department, ContactConsts.DepartmentMaxLength);
        IsPrimary = isPrimary;
        IsDecisionMaker = isDecisionMaker;
        OwnerId = ownerId;
        SetCreationAudit(tenantId, creatorId, now);
    }

    public Guid CustomerId { get; private set; }
    public string FullName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Mobile { get; private set; }
    public string? Position { get; private set; }
    public string? Department { get; private set; }
    public bool IsPrimary { get; private set; }
    public bool IsDecisionMaker { get; private set; }
    public string? LinkedInUrl { get; private set; }
    public string? Notes { get; private set; }
    public Guid? OwnerId { get; private set; }

    public void Update(
        Guid customerId,
        string fullName,
        string? email,
        string? phone,
        string? mobile,
        string? position,
        string? department,
        bool isPrimary,
        bool isDecisionMaker,
        string? linkedInUrl,
        string? notes,
        Guid? ownerId,
        Guid? modifierId,
        DateTimeOffset now)
    {
        CustomerId = customerId;
        FullName = Check.Length(Check.NotNullOrWhiteSpace(fullName, nameof(fullName)), nameof(fullName), ContactConsts.FullNameMaxLength);
        Email = NormalizeOptional(email, ContactConsts.EmailMaxLength);
        Phone = NormalizeOptional(phone, ContactConsts.PhoneMaxLength);
        Mobile = NormalizeOptional(mobile, ContactConsts.MobileMaxLength);
        Position = NormalizeOptional(position, ContactConsts.PositionMaxLength);
        Department = NormalizeOptional(department, ContactConsts.DepartmentMaxLength);
        IsPrimary = isPrimary;
        IsDecisionMaker = isDecisionMaker;
        LinkedInUrl = NormalizeOptional(linkedInUrl, ContactConsts.LinkedInUrlMaxLength);
        Notes = notes?.Trim();
        OwnerId = ownerId;
        SetModificationAudit(modifierId, now);
    }

    private static string? NormalizeOptional(string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value) ? null : Check.Length(value.Trim(), nameof(value), maxLength);
}

public interface IContactRepository : IRepository<Contact, Guid>
{
    Task<IReadOnlyList<Contact>> GetListByTenantAsync(Guid tenantId, Guid? customerId, string? search, int skipCount, int maxResultCount, CancellationToken cancellationToken = default);
    Task<long> GetCountByTenantAsync(Guid tenantId, Guid? customerId, string? search, CancellationToken cancellationToken = default);
}
