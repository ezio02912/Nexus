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
            null,
            "InApp",
            "Welcome to Nexus",
            $"Tài khoản {integrationEvent.UserName} đã được tạo."), cancellationToken);
    }
}

public sealed class TenantCreatedHandler : IIntegrationEventHandler<TenantCreatedIntegrationEvent>
{
    private readonly AuditApiClient _audit;
    private readonly NotificationApiClient _notifications;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TenantCreatedHandler> _logger;

    public TenantCreatedHandler(
        AuditApiClient audit,
        NotificationApiClient notifications,
        IConfiguration configuration,
        ILogger<TenantCreatedHandler> logger)
    {
        _audit = audit;
        _notifications = notifications;
        _configuration = configuration;
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

        if (string.IsNullOrWhiteSpace(integrationEvent.ContactEmail))
        {
            return;
        }

        var portalUrl = _configuration["TenantPortal:BaseUrl"] ?? "http://localhost:5280/login";
        var body = $"""
            <html>
            <body style="font-family:Segoe UI,Arial,sans-serif;color:#0F172A;">
              <h2>Chào mừng đến với Nexus</h2>
              <p>Tenant <strong>{integrationEvent.Name}</strong> của bạn đã được tạo thành công.</p>
              <p>Mã tenant của bạn: <strong style="font-size:20px;">{integrationEvent.Code}</strong></p>
              <p>Bạn có thể đăng nhập bằng Google hoặc email/mật khẩu tại:</p>
              <p><a href="{portalUrl}">{portalUrl}</a></p>
            </body>
            </html>
            """;

        await _notifications.SendAsync(new CreateNotificationRequest(
            integrationEvent.CreatedTenantId,
            null,
            integrationEvent.ContactEmail,
            "Email",
            "Mã tenant Nexus của bạn",
            body), cancellationToken);
    }
}
