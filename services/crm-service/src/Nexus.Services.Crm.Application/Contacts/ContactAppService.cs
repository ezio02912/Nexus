using Nexus.ApiContracts.Dtos;
using Nexus.Services.Crm.Contracts.Contacts;
using Nexus.Services.Crm.Domain.Contacts;
using Nexus.Services.Crm.Domain.Customers;
using Nexus.SharedKernel.Context;

namespace Nexus.Services.Crm.Application.Contacts;

public sealed class ContactAppService : CrmAppServiceBase, IContactAppService
{
    private readonly IContactRepository _contactRepository;
    private readonly ICustomerRepository _customerRepository;

    public ContactAppService(
        IContactRepository contactRepository,
        ICustomerRepository customerRepository,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        ICorrelationContext correlationContext)
        : base(currentTenant, currentUser, correlationContext)
    {
        _contactRepository = contactRepository;
        _customerRepository = customerRepository;
    }

    public async Task<PagedResultDto<ContactDto>> GetListAsync(GetContactsInput input, CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();

        var items = await _contactRepository.GetListByTenantAsync(
            tenantId,
            input.CustomerId,
            input.Search,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        return new PagedResultDto<ContactDto>
        {
            TotalCount = await _contactRepository.GetCountByTenantAsync(tenantId, input.CustomerId, input.Search, cancellationToken),
            Items = items.Select(MapToDto).ToArray()
        };
    }

    public async Task<ContactDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetAsync(id, cancellationToken);
        EnsureTenantAccess(contact);
        return MapToDto(contact);
    }

    public async Task<ContactDto> CreateAsync(CreateContactDto input, CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        await EnsureCustomerExistsAsync(tenantId, input.CustomerId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var contact = new Contact(
            Guid.NewGuid(),
            tenantId,
            input.CustomerId,
            input.FullName,
            input.Email,
            input.Phone,
            input.Mobile,
            input.Position,
            input.Department,
            input.IsPrimary,
            input.IsDecisionMaker,
            input.OwnerId,
            CurrentUser.Id,
            now);

        await _contactRepository.InsertAsync(contact, cancellationToken);
        return MapToDto(contact);
    }

    public async Task<ContactDto> UpdateAsync(Guid id, UpdateContactDto input, CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetAsync(id, cancellationToken);
        EnsureTenantAccess(contact);

        var tenantId = GetRequiredTenantId();
        await EnsureCustomerExistsAsync(tenantId, input.CustomerId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        contact.Update(
            input.CustomerId,
            input.FullName,
            input.Email,
            input.Phone,
            input.Mobile,
            input.Position,
            input.Department,
            input.IsPrimary,
            input.IsDecisionMaker,
            input.LinkedInUrl,
            input.Notes,
            input.OwnerId,
            CurrentUser.Id,
            now);

        await _contactRepository.UpdateAsync(contact, cancellationToken);
        return MapToDto(contact);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.FindAsync(id, cancellationToken);
        if (contact is null)
        {
            return;
        }

        EnsureTenantAccess(contact);
        await _contactRepository.DeleteAsync(id, cancellationToken);
    }

    private async Task EnsureCustomerExistsAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.FindAsync(customerId, cancellationToken);
        if (customer is null || customer.TenantId != tenantId)
        {
            throw new KeyNotFoundException($"Customer with id '{customerId}' was not found.");
        }
    }

    private static ContactDto MapToDto(Contact contact)
    {
        return new ContactDto
        {
            Id = contact.Id,
            TenantId = contact.TenantId,
            CustomerId = contact.CustomerId,
            FullName = contact.FullName,
            Email = contact.Email,
            Phone = contact.Phone,
            Mobile = contact.Mobile,
            Position = contact.Position,
            Department = contact.Department,
            IsPrimary = contact.IsPrimary,
            IsDecisionMaker = contact.IsDecisionMaker,
            LinkedInUrl = contact.LinkedInUrl,
            Notes = contact.Notes,
            OwnerId = contact.OwnerId,
            CreationTime = contact.CreationTime,
            CreatorId = contact.CreatorId,
            LastModificationTime = contact.LastModificationTime,
            LastModifierId = contact.LastModifierId,
            ConcurrencyStamp = contact.ConcurrencyStamp
        };
    }
}
