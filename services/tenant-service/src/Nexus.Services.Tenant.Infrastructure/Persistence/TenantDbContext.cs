using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using TenantAggregate = Nexus.Services.Tenant.Domain.Tenants.Tenant;

namespace Nexus.Services.Tenant.Infrastructure.Persistence;

public sealed class TenantDbContext : NexusDbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options)
    {
    }

    public DbSet<TenantAggregate> Tenants => Set<TenantAggregate>();
    public DbSet<Domain.Billing.SubscriptionPayment> SubscriptionPayments => Set<Domain.Billing.SubscriptionPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
