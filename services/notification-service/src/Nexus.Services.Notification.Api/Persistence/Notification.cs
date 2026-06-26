using Microsoft.EntityFrameworkCore;
using Nexus.BuildingBlocks.EntityFrameworkCore;
using Nexus.SharedKernel.Domain;

namespace Nexus.Services.Notification.Api.Persistence;

public sealed class Notification : NexusEntity<Guid>
{
    private Notification()
    {
        Channel = string.Empty;
        Subject = string.Empty;
        Body = string.Empty;
        Status = "Sent";
    }

    public Notification(Guid id, Guid? tenantId, Guid? recipientUserId, string? recipientEmail, string channel, string subject, string body, DateTimeOffset createdAt)
    {
        Id = id;
        TenantId = tenantId;
        RecipientUserId = recipientUserId;
        RecipientEmail = recipientEmail;
        Channel = channel;
        Subject = subject;
        Body = body;
        Status = "Sent";
        CreatedAt = createdAt;
    }

    public Guid? TenantId { get; private set; }
    public Guid? RecipientUserId { get; private set; }
    public string? RecipientEmail { get; private set; }
    public string Channel { get; private set; }
    public string Subject { get; private set; }
    public string Body { get; private set; }
    public string Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }

    public void MarkRead(DateTimeOffset now)
    {
        Status = "Read";
        ReadAt = now;
    }
}

public sealed class NotificationDbContext : NexusDbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(builder =>
        {
            builder.ToTable("notifications");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Channel).HasMaxLength(32).IsRequired();
            builder.Property(x => x.RecipientEmail).HasMaxLength(256);
            builder.Property(x => x.Subject).HasMaxLength(512).IsRequired();
            builder.Property(x => x.Body).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
            builder.HasIndex(x => new { x.TenantId, x.RecipientUserId });
        });

        base.OnModelCreating(modelBuilder);
    }
}
