using Nexus.Services.Identity.Domain.Users;

namespace Nexus.Services.Identity.Application.Users;

public sealed record TokenResult(string AccessToken, DateTimeOffset ExpiresAt, string RefreshToken);

/// <summary>
/// Issues and refreshes JWT access tokens together with persisted refresh tokens.
/// </summary>
public interface ITokenService
{
    Task<TokenResult> IssueAsync(User user, CancellationToken cancellationToken = default);

    Task<(TokenResult Tokens, User User)?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
}
