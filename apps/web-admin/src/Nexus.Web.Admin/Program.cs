using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Nexus.Web.Admin.Components;
using Nexus.Web.Admin.Services;
using BootstrapBlazor.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();

// Render Vietnamese (and other non-ASCII) characters literally instead of HTML-encoding them
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

// Register BootstrapBlazor and enable Vietnamese as the primary culture,
// falling back to English for any keys missing in vi.json.
builder.Services.AddBootstrapBlazor(options =>
{
    options.SupportedCultures = ["vi", "en"];
    options.FallbackCulture = "en";
});

// Default the whole application to Vietnamese while still supporting English.
var supportedCultures = new[] { "vi", "en" };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture("vi");
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
});

builder.Services.Configure<CoreServiceOptions>(builder.Configuration.GetSection("CoreServices"));
builder.Services.Configure<ObservabilityOptions>(builder.Configuration.GetSection(ObservabilityOptions.SectionName));
builder.Services.AddScoped<CoreApiClient>();
builder.Services.AddScoped<MonitoringService>();
builder.Services.AddScoped<AdminSessionService>();
builder.Services.AddScoped<TenantLookupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseRequestLocalization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Persists the selected UI language in the standard localization cookie, then returns
// the user to the page they came from. Used by the header language switcher.
app.MapGet("/set-culture", (string? culture, string? redirect, HttpContext context) =>
{
    if (!string.IsNullOrWhiteSpace(culture))
    {
        context.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });
    }

    return Results.LocalRedirect(string.IsNullOrWhiteSpace(redirect) ? "/" : redirect);
});

app.Run();
