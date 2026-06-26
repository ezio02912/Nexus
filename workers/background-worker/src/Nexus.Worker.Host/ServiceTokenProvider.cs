using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Nexus.BuildingBlocks.Web;
using Nexus.BuildingBlocks.Web.Auth;

namespace Nexus.Worker.Host;

/// <summary>
/// Issues a self-signed service JWT (with the wildcard permission) so the worker can call the
/// platform's authenticated APIs on behalf of the system.
/// </summary>
public sealed class ServiceTokenProvider
{
    private readonly NexusJwtOptions _options;
    private readonly SymmetricSecurityKey _key;
    private string? _token;
    private DateTimeOffset _expiresAt;

    public ServiceTokenProvider(IOptions<NexusJwtOptions> options)
    {
        _options = options.Value;
        var key = string.IsNullOrWhiteSpace(_options.SigningKey)
            ? "nexus-development-signing-key-please-override-0123456789"
            : _options.SigningKey;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }

    public string GetToken()
    {
        if (_token is not null && _expiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            return _token;
        }

        var now = DateTimeOffset.UtcNow;
        _expiresAt = now.AddHours(1);

        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(NexusClaimTypes.UserId, Guid.Empty.ToString()));
        identity.AddClaim(new Claim(NexusClaimTypes.UserName, "background-worker"));
        identity.AddClaim(new Claim(NexusClaimTypes.Permission, "*"));

        var handler = new JsonWebTokenHandler { SetDefaultTimesOnTokenCreation = false };
        _token = handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = identity,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = _expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
        });

        return _token;
    }
}

public sealed class ServiceTokenHandler : DelegatingHandler
{
    private readonly ServiceTokenProvider _tokenProvider;

    public ServiceTokenHandler(ServiceTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenProvider.GetToken());
        return base.SendAsync(request, cancellationToken);
    }
}
