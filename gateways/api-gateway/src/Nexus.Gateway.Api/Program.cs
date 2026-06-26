using Nexus.BuildingBlocks.Observability;

var builder = WebApplication.CreateBuilder(args);
builder.AddNexusObservability("api-gateway");

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowed(_ => true)
        .AllowCredentials()));

var app = builder.Build();

app.UseCors();

app.MapGet("/", () => Results.Ok(new { Service = "Nexus API Gateway", Status = "Running" }));
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", CheckedAt = DateTimeOffset.UtcNow }));

// Forwards requests (including the Authorization header) to the downstream services.
app.MapReverseProxy();

app.MapNexusObservability();
app.Run();
