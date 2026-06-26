using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Nexus.BuildingBlocks.Web;
using Nexus.BuildingBlocks.Web.Auth;
using Nexus.Services.Identity.Application.Users;
using Nexus.Services.Identity.Domain.Users;
using Nexus.Services.Identity.Infrastructure.Persistence;

namespace Nexus.Services.Identity.Infrastructure.Users;

public sealed class JwtTokenService : ITokenService
{
    private static readonly string[] AdminRoles = ["ADMIN", "SUPERADMIN"];

    private readonly IdentityDbContext _dbContext;
    private readonly IUserPermissionResolver _permissionResolver;
    private readonly NexusJwtOptions _options;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(IdentityDbContext dbContext, IUserPermissionResolver permissionResolver, IOptions<NexusJwtOptions> options)
    {
        _dbContext = dbContext;
        _permissionResolver = permissionResolver;
        _options = options.Value;
        var key = string.IsNullOrWhiteSpace(_options.SigningKey)
            ? "nexus-development-signing-key-please-override-0123456789"
            : _options.SigningKey;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }

    public async Task<TokenResult> IssueAsync(User user, CancellationToken cancellationToken = default)
    {
        var roles = user.Roles.Select(x => x.RoleName).ToArray();
        var permissions = await ResolvePermissionsAsync(user.TenantId!.Value, roles, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);

        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(NexusClaimTypes.UserId, user.Id.ToString()));
        identity.AddClaim(new Claim(NexusClaimTypes.UserName, user.UserName));
        identity.AddClaim(new Claim(NexusClaimTypes.TenantId, user.TenantId!.Value.ToString()));
        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(NexusClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            identity.AddClaim(new Claim(NexusClaimTypes.Permission, permission));
        }

        var handler = new JsonWebTokenHandler { SetDefaultTimesOnTokenCreation = false };
        var accessToken = handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = identity,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
        });

        var refreshToken = await CreateRefreshTokenAsync(user, now, cancellationToken);
        return new TokenResult(accessToken, expiresAt, refreshToken);
    }

    public async Task<(TokenResult Tokens, User User)?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hash = HashToken(refreshToken);
        var now = DateTimeOffset.UtcNow;

        var stored = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);

        if (stored is null || !stored.IsActive(now))
        {
            return null;
        }

        var user = await _dbContext.Users.Include(x => x.Roles).SingleOrDefaultAsync(x => x.Id == stored.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        stored.Revoke(now);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var tokens = await IssueAsync(user, cancellationToken);
        return (tokens, user);
    }

    private async Task<IReadOnlyCollection<string>> ResolvePermissionsAsync(Guid tenantId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
    {
        if (roles.Any(r => AdminRoles.Contains(r, StringComparer.OrdinalIgnoreCase)))
        {
            return ["*"];
        }

        return await _permissionResolver.GetPermissionsAsync(tenantId, roles, cancellationToken);
    }

    private async Task<string> CreateRefreshTokenAsync(User user, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var entity = new RefreshToken(
            Guid.NewGuid(),
            user.TenantId!.Value,
            user.Id,
            HashToken(raw),
            now.AddDays(_options.RefreshTokenDays),
            now);

        await _dbContext.RefreshTokens.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return raw;
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
