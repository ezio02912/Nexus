using Microsoft.IdentityModel.JsonWebTokens;

namespace Nexus.Web.Tenant.Services;

public static class JwtPermissionReader
{
    public static IReadOnlyCollection<string> ReadPermissions(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return [];
        }

        try
        {
            var handler = new JsonWebTokenHandler();
            var token = handler.ReadJsonWebToken(accessToken);
            return token.Claims
                .Where(x => x.Type == "permission")
                .Select(x => x.Value)
                .ToArray();
        }
        catch
        {
            return [];
        }
    }
}
