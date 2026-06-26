using Microsoft.Extensions.Logging;
using Nexus.BuildingBlocks.Messaging;
using Nexus.EventContracts.Identity;
using Nexus.EventContracts.Tenants;

namespace Nexus.Worker.Host;

public sealed class UserCreatedHandler : IIntegrationEventHandler<UserCreatedIntegrationEvent>
{
    private readonly AuditApiClient _audit;
    private readonly NotificationApiClient _notifications;
    private readonly ILogger<UserCreatedHandler> _logger;

    public UserCreatedHandler(AuditApiClient audit, NotificationApiClient notifications, ILogger<UserCreatedHandler> logger)
    {
        _audit = audit;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task HandleAsync(UserCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UserCreated for {UserId}.", integrationEvent.UserId);

        await _audit.WriteAsync(new CreateAuditLogRequest(
            integrationEvent.TenantId,
            integrationEvent.UserId,
            "background-worker",
            "User",
            integrationEvent.UserId.ToString(),
            "Create",
            $"User '{integrationEvent.UserName}' created.",
            integrationEvent.CorrelationId), cancellationToken);

        await _notifications.SendAsync(new CreateNotificationRequest(
            integrationEvent.TenantId,
            integrationEvent.UserId,
            "InApp",
            "Welcome to Nexus",
            $"Tài khoản {integrationEvent.UserName} đã được tạo."), cancellationToken);
    }
}

public sealed class TenantCreatedHandler : IIntegrationEventHandler<TenantCreatedIntegrationEvent>
{
    private readonly AuditApiClient _audit;
    private readonly ILogger<TenantCreatedHandler> _logger;

    public TenantCreatedHandler(AuditApiClient audit, ILogger<TenantCreatedHandler> logger)
    {
        _audit = audit;
        _logger = logger;
    }

    public async Task HandleAsync(TenantCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling TenantCreated for {TenantId}.", integrationEvent.CreatedTenantId);

        await _audit.WriteAsync(new CreateAuditLogRequest(
            integrationEvent.CreatedTenantId,
            null,
            "background-worker",
            "Tenant",
            integrationEvent.CreatedTenantId.ToString(),
            "Create",
            $"Tenant '{integrationEvent.Code}' created.",
            integrationEvent.CorrelationId), cancellationToken);
    }
}
