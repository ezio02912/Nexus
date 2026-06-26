using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Nexus.Web.Tenant.Components;
using Nexus.Web.Tenant.Services;
using BootstrapBlazor.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();

builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

builder.Services.AddBootstrapBlazor(options =>
{
    options.SupportedCultures = ["vi", "en"];
    options.FallbackCulture = "vi";
});

var supportedCultures = new[] { "vi", "en" };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture("vi");
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
});

builder.Services.Configure<TenantPortalOptions>(builder.Configuration.GetSection("CoreServices"));
builder.Services.AddScoped<TenantPortalApiClient>();
builder.Services.AddScoped<CrmApiClient>();
builder.Services.AddScoped<MasterDataApiClient>();
builder.Services.AddScoped<TenantSessionService>();
builder.Services.AddScoped<OnboardingStateService>();
builder.Services.AddScoped<PendingAuthStateService>();

var googleClientId = builder.Configuration["Google:ClientId"];
var googleClientSecret = builder.Configuration["Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LoginPath = "/login";
        })
        .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
            options.SaveTokens = true;
            // openid is required for Google to return id_token in the token response
            options.Scope.Add("openid");
            options.Scope.Add("email");
            options.Scope.Add("profile");
        });
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseAntiforgery();

if (!string.IsNullOrWhiteSpace(googleClientId))
{
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/auth/google/login", async ctx =>
    {
        await ctx.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
        {
            RedirectUri = "/auth/google/callback"
        });
    });

    app.MapGet("/auth/google/callback", async (HttpContext ctx, IHttpClientFactory httpClientFactory, IConfiguration configuration, PendingAuthStateService pendingAuth) =>
    {
        var authenticateResult = await ctx.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded)
        {
            return Results.Redirect("/login?error=google");
        }

        var idToken = authenticateResult.Properties?.GetTokenValue("id_token");
        var accessToken = authenticateResult.Properties?.GetTokenValue("access_token");
        if (string.IsNullOrWhiteSpace(idToken) && string.IsNullOrWhiteSpace(accessToken))
        {
            return Results.Redirect("/login?error=google-token");
        }

        var identityUrl = configuration["CoreServices:Identity"] ?? "http://localhost:7202";
        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync($"{identityUrl.TrimEnd('/')}/api/auth/google", new GoogleAuthRequest(idToken, accessToken));
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Results.Redirect("/login?error=google-auth-outdated");
            }

            return Results.Redirect("/login?error=google-auth");
        }

        var result = await response.Content.ReadFromJsonAsync<GoogleAuthResult>();
        await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (result is null)
        {
            return Results.Redirect("/login?error=google-auth");
        }

        if (string.Equals(result.Status, "NeedOnboarding", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Redirect($"/onboarding?token={Uri.EscapeDataString(result.OnboardingToken ?? "")}&email={Uri.EscapeDataString(result.Email ?? "")}&name={Uri.EscapeDataString(result.DisplayName ?? "")}");
        }

        if (result.Login is null)
        {
            return Results.Redirect("/login?error=google-auth");
        }

        pendingAuth.Set(new PendingAuthPayload
        {
            Mode = "Login",
            Login = result.Login,
            UserName = result.Login.UserName ?? result.Login.Email ?? result.Email
        });
        return Results.Redirect("/auth/complete");
    });
}

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
