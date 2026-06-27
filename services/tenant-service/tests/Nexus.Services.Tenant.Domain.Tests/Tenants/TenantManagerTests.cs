using Nexus.Services.Tenant.Domain.Tenants;
using Nexus.SharedKernel.Context;
using Nexus.SharedKernel.Exceptions;
using DomainTenant = Nexus.Services.Tenant.Domain.Tenants.Tenant;

namespace Nexus.Services.Tenant.Domain.Tests.Tenants;

public sealed class TenantManagerTests
{
    private static readonly Guid CurrentUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task CreateAsync_normalizes_code_and_persists_new_tenant()
    {
        var repository = new InMemoryTenantRepository();
        var manager = new TenantManager(repository, new StubCurrentUser(CurrentUserId));

        var tenant = await manager.CreateAsync(
            " acme ",
            "Acme Co",
            "  123 Main  ",
            "  0909  ",
            " Jane Doe ",
            " Admin@Example.COM ");

        Assert.Same(tenant, repository.InsertedTenant);
        Assert.Equal("ACME", tenant.Code);
        Assert.Equal("Acme Co", tenant.Name);
        Assert.Equal("123 Main", tenant.Address);
        Assert.Equal("0909", tenant.Phone);
        Assert.Equal("Jane Doe", tenant.RepresentativeName);
        Assert.Equal("admin@example.com", tenant.ContactEmail);
        Assert.Equal(CurrentUserId, tenant.CreatorId);
    }

    [Fact]
    public async Task CreateAsync_throws_when_code_already_exists()
    {
        var repository = new InMemoryTenantRepository
        {
            ExistingTenant = new DomainTenant(Guid.NewGuid(), "ACME", "Existing", null, DateTimeOffset.UtcNow)
        };
        var manager = new TenantManager(repository, new StubCurrentUser(CurrentUserId));

        var exception = await Assert.ThrowsAsync<NexusBusinessException>(() => manager.CreateAsync(" acme ", "Acme Co"));

        Assert.Equal(TenantErrorCodes.AlreadyExists, exception.Code);
        Assert.Null(repository.InsertedTenant);
    }

    private sealed class StubCurrentUser(Guid? id) : ICurrentUser
    {
        public Guid? Id { get; } = id;
        public string? UserName => "tester";
        public IReadOnlyCollection<string> Permissions => [];
        public bool IsAuthenticated => Id.HasValue;
    }

    private sealed class InMemoryTenantRepository : ITenantRepository
    {
        public DomainTenant? ExistingTenant { get; init; }
        public DomainTenant? InsertedTenant { get; private set; }

        public Task<DomainTenant?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExistingTenant is not null && ExistingTenant.Code == code ? ExistingTenant : null);
        }

        public Task<DomainTenant> InsertAsync(DomainTenant entity, CancellationToken cancellationToken = default)
        {
            InsertedTenant = entity;
            return Task.FromResult(entity);
        }

        public Task<DomainTenant?> FindAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<DomainTenant?>(null);
        public Task<DomainTenant> GetAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<DomainTenant>> GetListAsync(int skipCount = 0, int maxResultCount = 50, string? sorting = null, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<DomainTenant>>([]);
        public Task<long> GetCountAsync(CancellationToken cancellationToken = default) => Task.FromResult(0L);
        public Task<DomainTenant> UpdateAsync(DomainTenant entity, CancellationToken cancellationToken = default) => Task.FromResult(entity);
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
