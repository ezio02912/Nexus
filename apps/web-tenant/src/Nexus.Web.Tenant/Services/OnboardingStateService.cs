using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Nexus.Web.Tenant.Services;

public sealed class OnboardingStateService(ProtectedLocalStorage storage)
{
    private const string StorageKey = "nexus.tenant.onboarding";

    public string? OnboardingToken { get; private set; }
    public string? Email { get; private set; }
    public string? DisplayName { get; private set; }

    public async Task LoadAsync()
    {
        try
        {
            var stored = await storage.GetAsync<OnboardingPersistedState>(StorageKey);
            if (stored is { Success: true, Value: { } state })
            {
                OnboardingToken = state.OnboardingToken;
                Email = state.Email;
                DisplayName = state.DisplayName;
            }
        }
        catch
        {
            // Protected storage may be unavailable during prerender.
        }
    }

    public async Task SaveAsync(string onboardingToken, string email, string? displayName)
    {
        OnboardingToken = onboardingToken;
        Email = email;
        DisplayName = displayName;
        await storage.SetAsync(StorageKey, new OnboardingPersistedState(onboardingToken, email, displayName));
    }

    public async Task ClearAsync()
    {
        OnboardingToken = null;
        Email = null;
        DisplayName = null;
        await storage.DeleteAsync(StorageKey);
    }

    public bool IsReady => !string.IsNullOrWhiteSpace(OnboardingToken) && !string.IsNullOrWhiteSpace(Email);

    private sealed record OnboardingPersistedState(string OnboardingToken, string Email, string? DisplayName);
}
