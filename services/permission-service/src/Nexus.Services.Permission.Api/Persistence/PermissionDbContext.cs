using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;

namespace Nexus.Services.Permission.Api.Persistence;

public sealed class PermissionDbContext : NexusDbContext
{
    public PermissionDbContext(DbContextOptions<PermissionDbContext> options) : base(options)
    {
    }

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePermission>(builder =>
        {
            builder.ToTable("role_permissions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RoleName).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Permission).HasMaxLength(256).IsRequired();
            builder.HasIndex(x => new { x.RoleName, x.Permission }).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}
