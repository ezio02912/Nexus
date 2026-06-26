namespace Nexus.Web.Admin.Services;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string GrafanaUrl { get; set; } = "http://localhost:3000";
    public string GrafanaLogsDashboardUrl { get; set; } = "http://localhost:3000/d/nexus-logs/nexus-platform-logs";
    public string GrafanaOverviewDashboardUrl { get; set; } = "http://localhost:3000/d/nexus-overview/nexus-platform-overview";
    public string PrometheusUrl { get; set; } = "http://localhost:9090";
    public string RabbitMqManagementUrl { get; set; } = "http://localhost:15672";
    public string MailpitUrl { get; set; } = "http://localhost:8025";
    public string MinioConsoleUrl { get; set; } = "http://localhost:9001";
}

public sealed class ServiceHealthStatus
{
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string HealthUrl { get; set; } = "";
    public bool IsHealthy { get; set; }
    public int? StatusCode { get; set; }
    public string? Detail { get; set; }
    public TimeSpan ResponseTime { get; set; }
}

public sealed class MonitoringService
{
    private static readonly (string Name, string DisplayName, string HealthUrl)[] Services =
    [
        ("api-gateway", "API Gateway", "http://localhost:7200/health"),
        ("tenant-service", "Tenant", "http://localhost:7201/health"),
        ("identity-service", "Identity", "http://localhost:7202/health"),
        ("permission-service", "Permission", "http://localhost:7203/health"),
        ("audit-service", "Audit", "http://localhost:7204/health"),
        ("file-service", "File", "http://localhost:7205/health"),
        ("numbering-service", "Numbering", "http://localhost:7206/health"),
        ("workflow-service", "Workflow", "http://localhost:7207/health"),
        ("crm-service", "CRM", "http://localhost:7208/health"),
        ("sales-service", "Sales", "http://localhost:7209/health"),
        ("notification-service", "Notification", "http://localhost:7210/health"),
        ("masterdata-service", "Master Data", "http://localhost:7211/health"),
    ];

    private readonly IHttpClientFactory _httpClientFactory;

    public MonitoringService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<ServiceHealthStatus>> GetServiceHealthAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient(nameof(MonitoringService));
        client.Timeout = TimeSpan.FromSeconds(5);

        var tasks = Services.Select(async service =>
        {
            var started = DateTimeOffset.UtcNow;
            try
            {
                using var response = await client.GetAsync(service.HealthUrl, cancellationToken);
                var elapsed = DateTimeOffset.UtcNow - started;
                var detail = response.IsSuccessStatusCode
                    ? null
                    : await response.Content.ReadAsStringAsync(cancellationToken);

                return new ServiceHealthStatus
                {
                    Name = service.Name,
                    DisplayName = service.DisplayName,
                    HealthUrl = service.HealthUrl,
                    IsHealthy = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Detail = detail,
                    ResponseTime = elapsed
                };
            }
            catch (Exception ex)
            {
                var elapsed = DateTimeOffset.UtcNow - started;
                return new ServiceHealthStatus
                {
                    Name = service.Name,
                    DisplayName = service.DisplayName,
                    HealthUrl = service.HealthUrl,
                    IsHealthy = false,
                    Detail = ex.Message,
                    ResponseTime = elapsed
                };
            }
        });

        return await Task.WhenAll(tasks);
    }
}
