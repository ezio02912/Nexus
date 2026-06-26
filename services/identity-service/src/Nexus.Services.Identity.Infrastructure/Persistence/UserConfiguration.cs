using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Services.Identity.Domain.Users;

namespace Nexus.Services.Identity.Infrastructure.Persistence;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserName).HasMaxLength(UserConsts.UserNameMaxLength).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(UserConsts.EmailMaxLength).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(512);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(64).IsConcurrencyToken();

        builder.HasIndex(x => new { x.TenantId, x.UserName }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();

        var rolesNavigation = builder.Metadata.FindNavigation(nameof(User.Roles))!;
        rolesNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Roles)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RoleName).HasMaxLength(UserConsts.RoleNameMaxLength).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.RoleName }).IsUnique();
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).IsRequired();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.TokenHash);
    }
}
