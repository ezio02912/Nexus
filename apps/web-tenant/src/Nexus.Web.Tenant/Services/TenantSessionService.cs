using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Nexus.ApiContracts.Permissions;

namespace Nexus.Web.Tenant.Services;

public sealed class TenantSessionService(ProtectedLocalStorage storage)
{
    private const string StorageKey = "nexus.tenant.session";
    private HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);

    public LoginResult? Login { get; private set; }
    public TenantDto? Tenant { get; private set; }
    public string? UserName { get; private set; }

    public bool IsInitialized { get; private set; }

    /// <summary>Raised when the active tenant (modules, subscription) changes so the shell can rebuild its menu.</summary>
    public event Action? TenantChanged;

    public bool IsAuthenticated => Login is not null && Tenant is not null && Login.ExpiresAt > DateTimeOffset.UtcNow;
    public Guid? TenantId => Tenant?.Id;

    public IReadOnlyCollection<string> Permissions => _permissions;

    public IReadOnlyCollection<string> EnabledModules =>
        Tenant?.Modules?.Where(x => x.IsEnabled).Select(x => x.ModuleCode.ToUpperInvariant()).ToArray() ?? [];

    public bool HasModule(string moduleCode) => EnabledModules.Contains(moduleCode.ToUpperInvariant());

    public bool IsGranted(string permission) => NexusPermissionLegacy.IsGranted(_permissions, permission);

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
                RefreshPermissions();
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
        RefreshPermissions();
        IsInitialized = true;
        await storage.SetAsync(StorageKey, new PersistedSession(userName, login, tenant));
        TenantChanged?.Invoke();
    }

    public async Task SignOutAsync()
    {
        UserName = null;
        Login = null;
        Tenant = null;
        _permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await storage.DeleteAsync(StorageKey);
    }

    public async Task UpdateTenantAsync(TenantDto tenant)
    {
        if (Login is null || UserName is null)
        {
            return;
        }

        Tenant = tenant;
        await storage.SetAsync(StorageKey, new PersistedSession(UserName, Login, tenant));
        TenantChanged?.Invoke();
    }

    private void RefreshPermissions()
    {
        _permissions = new HashSet<string>(
            JwtPermissionReader.ReadPermissions(Login?.AccessToken),
            StringComparer.OrdinalIgnoreCase);
    }

    private sealed record PersistedSession(string UserName, LoginResult Login, TenantDto Tenant);
}
