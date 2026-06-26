using System.Net.Http.Json;

namespace Nexus.Worker.Host;

public sealed record CreateAuditLogRequest(
    Guid? TenantId,
    Guid? UserId,
    string ServiceName,
    string EntityName,
    string? EntityId,
    string Action,
    string? Summary,
    string? CorrelationId);

public sealed record CreateNotificationRequest(
    Guid? TenantId,
    Guid? RecipientUserId,
    string? RecipientEmail,
    string Channel,
    string Subject,
    string Body);

public sealed class AuditApiClient
{
    private readonly HttpClient _httpClient;

    public AuditApiClient(HttpClient httpClient) => _httpClient = httpClient;

    public Task WriteAsync(CreateAuditLogRequest request, CancellationToken cancellationToken = default) =>
        _httpClient.PostAsJsonAsync("/api/audit-logs", request, cancellationToken);
}

public sealed class NotificationApiClient
{
    private readonly HttpClient _httpClient;

    public NotificationApiClient(HttpClient httpClient) => _httpClient = httpClient;

    public Task SendAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default) =>
        _httpClient.PostAsJsonAsync("/api/notifications", request, cancellationToken);
}
