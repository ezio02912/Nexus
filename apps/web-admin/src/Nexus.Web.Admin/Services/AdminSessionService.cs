using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Nexus.Web.Admin.Services;

public sealed class AdminSessionService(ProtectedLocalStorage storage)
{
    private const string StorageKey = "nexus.admin.session";

    public LoginResult? Login { get; private set; }
    public string? UserName { get; private set; }

    // True once we have attempted to restore the session from browser storage.
    // The layout uses this to avoid redirecting to login before rehydration runs.
    public bool IsInitialized { get; private set; }

    public bool IsAuthenticated => Login is not null && Login.ExpiresAt > DateTimeOffset.UtcNow;
    public Guid? TenantId => Login?.TenantId;
    public Guid? UserId => Login?.UserId;

    // Restore the session from browser local storage so it survives full page reloads
    // (e.g. switching language) and newly opened browser tabs. ProtectedLocalStorage
    // relies on JS interop, so this must be called after the first render.
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
            }
            else if (stored.Success)
            {
                // Stored token is missing or expired; drop it.
                await storage.DeleteAsync(StorageKey);
            }
        }
        catch
        {
            // Browser storage is unavailable (e.g. during prerender); treat as signed out.
        }

        IsInitialized = true;
    }

    public async Task SignInAsync(string userName, LoginResult login)
    {
        UserName = userName;
        Login = login;
        IsInitialized = true;
        await storage.SetAsync(StorageKey, new PersistedSession(userName, login));
    }

    public async Task SignOutAsync()
    {
        UserName = null;
        Login = null;
        await storage.DeleteAsync(StorageKey);
    }

    private sealed record PersistedSession(string UserName, LoginResult Login);
}
