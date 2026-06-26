using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.Services.Identity.Domain.Onboarding;
using Nexus.Services.Identity.Domain.Users;

namespace Nexus.Services.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext : NexusDbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();

    public DbSet<OnboardingSession> OnboardingSessions => Set<OnboardingSession>();

    public DbSet<TenantRegistration> TenantRegistrations => Set<TenantRegistration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
