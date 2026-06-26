using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Nexus.Web.Tenant.Services;

public sealed class TenantSessionService(ProtectedLocalStorage storage)
{
    private const string StorageKey = "nexus.tenant.session";

    public LoginResult? Login { get; private set; }
    public TenantDto? Tenant { get; private set; }
    public string? UserName { get; private set; }

    public bool IsInitialized { get; private set; }

    public bool IsAuthenticated => Login is not null && Tenant is not null && Login.ExpiresAt > DateTimeOffset.UtcNow;
    public Guid? TenantId => Tenant?.Id;

    public IReadOnlyCollection<string> EnabledModules =>
        Tenant?.Modules?.Where(x => x.IsEnabled).Select(x => x.ModuleCode.ToUpperInvariant()).ToArray() ?? [];

    public bool HasModule(string moduleCode) => EnabledModules.Contains(moduleCode.ToUpperInvariant());

    public async Task InitializeAsync()
    {
        if (IsInitialized)
        {
            return;
        }

        try
        {
            var stored = await storage.GetAsync<PersistedSession>(StorageKey);
            if (stored is { Success: true, Value: { } session }
                && session.Login.ExpiresAt > DateTimeOffset.UtcNow)
            {
                UserName = session.UserName;
                Login = session.Login;
                Tenant = session.Tenant;
            }
            else if (stored.Success)
            {
                await storage.DeleteAsync(StorageKey);
            }
        }
        catch
        {
            // Browser storage is unavailable during prerender; treat as signed out.
        }

        IsInitialized = true;
    }

    public async Task SignInAsync(string userName, LoginResult login, TenantDto tenant)
    {
        UserName = userName;
        Login = login;
        Tenant = tenant;
        IsInitialized = true;
        await storage.SetAsync(StorageKey, new PersistedSession(userName, login, tenant));
    }

    public async Task SignOutAsync()
    {
        UserName = null;
        Login = null;
        Tenant = null;
        await storage.DeleteAsync(StorageKey);
    }

    private sealed record PersistedSession(string UserName, LoginResult Login, TenantDto Tenant);
}
