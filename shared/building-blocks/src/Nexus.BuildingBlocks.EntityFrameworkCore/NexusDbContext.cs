using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore.Configurations;
using Nexus.SharedKernel.Domain;
using Nexus.SharedKernel.Events;

namespace Nexus.BuildingBlocks.EntityFrameworkCore;

/// <summary>
/// Base DbContext for every Nexus service. Provides the shared outbox/inbox tables
/// and a global soft-delete query filter for full-audited aggregates.
/// </summary>
public abstract class NexusDbContext : DbContext
{
    protected NexusDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<EventOutboxMessage> OutboxMessages => Set<EventOutboxMessage>();

    public DbSet<EventInboxMessage> InboxMessages => Set<EventInboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new EventOutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new EventInboxMessageConfiguration());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Soft-delete is enforced globally for any aggregate that opts into auditing.
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var filter = Expression.Lambda(Expression.Not(property), parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }

            // Map every table and column to snake_case to follow the platform DB conventions.
            entityType.SetTableName(ToSnakeCase(entityType.GetTableName()));
            foreach (var property in entityType.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
        }
    }

    private static string? ToSnakeCase(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        var builder = new System.Text.StringBuilder(name.Length + 8);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && (!char.IsUpper(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1]))))
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(c));
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
