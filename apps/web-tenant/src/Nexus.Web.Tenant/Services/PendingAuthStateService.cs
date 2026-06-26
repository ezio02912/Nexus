using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace Nexus.Web.Tenant.Services;

public sealed class PendingAuthPayload
{
    public string? Mode { get; init; }
    public string? OnboardingToken { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public string? UserName { get; init; }
    public LoginResult? Login { get; init; }
    public TenantDto? Tenant { get; init; }
}

public sealed class PendingAuthStateService(IHttpContextAccessor httpContextAccessor, IDataProtectionProvider dataProtectionProvider)
{
    private const string CookieName = "nexus.tenant.pending-auth";
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("Nexus.Web.Tenant.PendingAuth");

    public void Set(PendingAuthPayload payload)
    {
        var context = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");
        var json = JsonSerializer.Serialize(payload);
        var protectedValue = _protector.Protect(json);
        context.Response.Cookies.Append(CookieName, protectedValue, new CookieOptions
        {
            HttpOnly = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromMinutes(10),
            IsEssential = true
        });
    }

    public PendingAuthPayload? TryRead()
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null || !context.Request.Cookies.TryGetValue(CookieName, out var protectedValue))
        {
            return null;
        }

        try
        {
            var json = _protector.Unprotect(protectedValue);
            return JsonSerializer.Deserialize<PendingAuthPayload>(json);
        }
        catch
        {
            return null;
        }
    }

    public PendingAuthPayload? Consume()
    {
        var payload = TryRead();
        if (payload is null)
        {
            return null;
        }

        var context = httpContextAccessor.HttpContext;
        if (context is not null && !context.Response.HasStarted)
        {
            context.Response.Cookies.Delete(CookieName);
        }

        return payload;
    }
}
