using Nexus.Services.Identity.Domain.Users;
using Nexus.SharedKernel.Context;
using Nexus.SharedKernel.Exceptions;

namespace Nexus.Services.Identity.Domain.Tests.Users;

public sealed class UserManagerTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid CurrentUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task CreateAsync_normalizes_user_data_hashes_password_and_assigns_distinct_roles()
    {
        var repository = new InMemoryUserRepository();
        var manager = new UserManager(repository, new StubPasswordHasher(), new StubCurrentUser(CurrentUserId));

        var user = await manager.CreateAsync(
            TenantId,
            "  jane.doe  ",
            "  Jane.Doe@Example.COM  ",
            "secret",
            [" admin ", "ADMIN", "sales"]);

        Assert.Same(user, repository.InsertedUser);
        Assert.Equal(TenantId, user.TenantId);
        Assert.Equal("JANE.DOE", user.UserName);
        Assert.Equal("jane.doe@example.com", user.Email);
        Assert.Equal("hashed:secret", user.PasswordHash);
        Assert.True(user.IsActive);
        Assert.Equal(CurrentUserId, user.CreatorId);
        Assert.Equal(CurrentUserId, user.LastModifierId);
        Assert.Equal(["ADMIN", "SALES"], user.Roles.Select(x => x.RoleName).Order());
    }

    [Fact]
    public async Task CreateAsync_throws_when_user_name_already_exists_in_tenant()
    {
        var repository = new InMemoryUserRepository
        {
            UserNameMatch = new User(Guid.NewGuid(), TenantId, "JANE.DOE", "existing@example.com", "hash", null, DateTimeOffset.UtcNow)
        };
        var manager = new UserManager(repository, new StubPasswordHasher(), new StubCurrentUser(CurrentUserId));

        var exception = await Assert.ThrowsAsync<NexusBusinessException>(() =>
            manager.CreateAsync(TenantId, "jane.doe", "new@example.com", "secret", []));

        Assert.Equal(UserErrorCodes.AlreadyExists, exception.Code);
        Assert.Null(repository.InsertedUser);
    }

    [Fact]
    public async Task CreateAsync_throws_when_email_already_exists_in_tenant()
    {
        var repository = new InMemoryUserRepository
        {
            EmailMatch = new User(Guid.NewGuid(), TenantId, "EXISTING", "jane.doe@example.com", "hash", null, DateTimeOffset.UtcNow)
        };
        var manager = new UserManager(repository, new StubPasswordHasher(), new StubCurrentUser(CurrentUserId));

        var exception = await Assert.ThrowsAsync<NexusBusinessException>(() =>
            manager.CreateAsync(TenantId, "jane.doe", " Jane.Doe@Example.COM ", "secret", []));

        Assert.Equal(UserErrorCodes.AlreadyExists, exception.Code);
        Assert.Null(repository.InsertedUser);
    }

    private sealed class StubCurrentUser(Guid? id) : ICurrentUser
    {
        public Guid? Id { get; } = id;
        public string? UserName => "tester";
        public IReadOnlyCollection<string> Permissions => [];
        public bool IsAuthenticated => Id.HasValue;
    }

    private sealed class StubPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => $"hashed:{password}";
        public bool VerifyPassword(string password, string passwordHash) => passwordHash == HashPassword(password);
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        public User? UserNameMatch { get; init; }
        public User? EmailMatch { get; init; }
        public User? InsertedUser { get; private set; }

        public Task<User?> FindByUserNameAsync(Guid tenantId, string userName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(UserNameMatch is not null && UserNameMatch.TenantId == tenantId && UserNameMatch.UserName == userName ? UserNameMatch : null);
        }

        public Task<User?> FindByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EmailMatch is not null && EmailMatch.TenantId == tenantId && EmailMatch.Email == email ? EmailMatch : null);
        }

        public Task<User> InsertAsync(User entity, CancellationToken cancellationToken = default)
        {
            InsertedUser = entity;
            return Task.FromResult(entity);
        }

        public Task<User?> FindAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
        public Task<User> GetAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<User>> GetListAsync(int skipCount = 0, int maxResultCount = 50, string? sorting = null, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<User>>([]);
        public Task<long> GetCountAsync(CancellationToken cancellationToken = default) => Task.FromResult(0L);
        public Task<User> UpdateAsync(User entity, CancellationToken cancellationToken = default) => Task.FromResult(entity);
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<User?> FindByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
    }
}
