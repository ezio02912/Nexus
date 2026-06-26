using Nexus.ApiContracts.Dtos;
using Nexus.EventContracts.Crm;
using Nexus.Services.Crm.Contracts.Customers;
using Nexus.Services.Crm.Domain;
using Nexus.Services.Crm.Domain.Customers;
using Nexus.Services.Crm.Domain.Enums;
using Nexus.SharedKernel.Context;
using Nexus.SharedKernel.Events;
using Nexus.SharedKernel.Exceptions;

namespace Nexus.Services.Crm.Application.Customers;

public sealed class CustomerAppService : CrmAppServiceBase, ICustomerAppService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IEventBus _eventBus;

    public CustomerAppService(
        ICustomerRepository customerRepository,
        IEventBus eventBus,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        ICorrelationContext correlationContext)
        : base(currentTenant, currentUser, correlationContext)
    {
        _customerRepository = customerRepository;
        _eventBus = eventBus;
    }

    public async Task<PagedResultDto<CustomerDto>> GetListAsync(GetCustomersInput input, CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        var status = input.Status?.ToString();

        var items = await _customerRepository.GetListByTenantAsync(
            tenantId,
            input.Search,
            status,
            input.OwnerId,
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            cancellationToken);

        return new PagedResultDto<CustomerDto>
        {
            TotalCount = await _customerRepository.GetCountByTenantAsync(tenantId, input.Search, status, input.OwnerId, cancellationToken),
            Items = items.Select(MapToDto).ToArray()
        };
    }

    public async Task<CustomerDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetAsync(id, cancellationToken);
        EnsureTenantAccess(customer);
        return MapToDto(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto input, CancellationToken cancellationToken = default)
    {
        var tenantId = GetRequiredTenantId();
        var now = DateTimeOffset.UtcNow;

        if (await _customerRepository.FindByCodeAsync(tenantId, input.Code, cancellationToken) is not null)
        {
            throw new NexusBusinessException(CrmErrorCodes.CustomerAlreadyExists, "Customer code already exists.");
        }

        var customer = new Customer(
            Guid.NewGuid(),
            tenantId,
            input.Code,
            input.Name,
            input.CustomerType,
            input.Email,
            input.Phone,
            CurrentUser.Id,
            now);

        await _customerRepository.InsertAsync(customer, cancellationToken);

        if (!string.IsNullOrWhiteSpace(input.Industry) || !string.IsNullOrWhiteSpace(input.City) || !string.IsNullOrWhiteSpace(input.Source))
        {
            customer.UpdateProfile(
                input.Name,
                input.CustomerType,
                input.Email,
                input.Phone,
                null,
                null,
                input.Industry,
                null,
                null,
                input.City,
                null,
                null,
                null,
                null,
                null,
                null,
                input.Source,
                CustomerStatus.Active,
                CurrentUser.Id,
                now);
            await _customerRepository.UpdateAsync(customer, cancellationToken);
        }

        await _eventBus.PublishAsync(new CustomerCreatedIntegrationEvent(
            Guid.NewGuid(),
            tenantId,
            now,
            ServiceName,
            CorrelationContext.CorrelationId,
            customer.Id,
            customer.Code,
            customer.Name), cancellationToken);

        return MapToDto(customer);
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerDto input, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetAsync(id, cancellationToken);
        EnsureTenantAccess(customer);

        var now = DateTimeOffset.UtcNow;
        customer.UpdateProfile(
            input.Name,
            input.CustomerType,
            input.Email,
            input.Phone,
            input.TaxCode,
            input.Website,
            input.Industry,
            input.AddressLine1,
            input.AddressLine2,
            input.City,
            input.State,
            input.PostalCode,
            input.Country,
            input.OwnerId,
            input.Description,
            input.Rating,
            input.Source,
            input.Status,
            CurrentUser.Id,
            now);

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        return MapToDto(customer);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.FindAsync(id, cancellationToken);
        if (customer is null)
        {
            return;
        }

        EnsureTenantAccess(customer);
        await _customerRepository.DeleteAsync(id, cancellationToken);
    }

    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            TenantId = customer.TenantId,
            Code = customer.Code,
            Name = customer.Name,
            CustomerType = customer.CustomerType,
            Email = customer.Email,
            Phone = customer.Phone,
            Status = customer.Status,
            TaxCode = customer.TaxCode,
            Website = customer.Website,
            Industry = customer.Industry,
            AddressLine1 = customer.AddressLine1,
            AddressLine2 = customer.AddressLine2,
            City = customer.City,
            State = customer.State,
            PostalCode = customer.PostalCode,
            Country = customer.Country,
            OwnerId = customer.OwnerId,
            Description = customer.Description,
            Rating = customer.Rating,
            Source = customer.Source,
            CreationTime = customer.CreationTime,
            CreatorId = customer.CreatorId,
            LastModificationTime = customer.LastModificationTime,
            LastModifierId = customer.LastModifierId,
            ConcurrencyStamp = customer.ConcurrencyStamp
        };
    }
}
