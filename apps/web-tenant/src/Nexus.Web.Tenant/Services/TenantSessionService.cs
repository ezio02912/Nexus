namespace Nexus.Web.Tenant.Services;

public sealed class TenantSessionService
{
    public LoginResult? Login { get; private set; }
    public TenantDto? Tenant { get; private set; }
    public string? UserName { get; private set; }

    public bool IsAuthenticated => Login is not null && Tenant is not null && Login.ExpiresAt > DateTimeOffset.UtcNow;
    public Guid? TenantId => Tenant?.Id;

    public IReadOnlyCollection<string> EnabledModules =>
        Tenant?.Modules?.Where(x => x.IsEnabled).Select(x => x.ModuleCode.ToUpperInvariant()).ToArray() ?? [];

    public bool HasModule(string moduleCode)
    {
        return EnabledModules.Contains(moduleCode.ToUpperInvariant());
    }

    public void SignIn(string userName, LoginResult login, TenantDto tenant)
    {
        UserName = userName;
        Login = login;
        Tenant = tenant;
    }

    public void SignOut()
    {
        UserName = null;
        Login = null;
        Tenant = null;
    }
}
