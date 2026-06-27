using Nexus.BuildingBlocks.Observability;
using Nexus.BuildingBlocks.Messaging;
using Nexus.BuildingBlocks.Web.Auth;
using Nexus.EventContracts.Identity;
using Nexus.EventContracts.Tenants;
using Nexus.Worker.Host;

var builder = Host.CreateApplicationBuilder(args);
builder.AddNexusWorkerObservability("background-worker");

builder.Services.Configure<NexusJwtOptions>(builder.Configuration.GetSection(NexusJwtOptions.SectionName));
builder.Services.AddSingleton<ServiceTokenProvider>();
builder.Services.AddTransient<ServiceTokenHandler>();

var auditUrl = builder.Configuration["Services:Audit"] ?? "http://localhost:7204";
var notificationUrl = builder.Configuration["Services:Notification"] ?? "http://localhost:7213";

builder.Services.AddHttpClient<AuditApiClient>(client => client.BaseAddress = new Uri(auditUrl))
    .AddHttpMessageHandler<ServiceTokenHandler>();
builder.Services.AddHttpClient<NotificationApiClient>(client => client.BaseAddress = new Uri(notificationUrl))
    .AddHttpMessageHandler<ServiceTokenHandler>();

builder.Services.AddNexusRabbitMqConsumer(
    builder.Configuration,
    "nexus.worker",
    typeof(UserCreatedIntegrationEvent).Assembly);

builder.Services.AddIntegrationEventHandler<UserCreatedIntegrationEvent, UserCreatedHandler>();
builder.Services.AddIntegrationEventHandler<TenantCreatedIntegrationEvent, TenantCreatedHandler>();
builder.Services.AddIntegrationEventHandler<SubscriptionChangedIntegrationEvent, SubscriptionChangedHandler>();

var host = builder.Build();
host.Run();
