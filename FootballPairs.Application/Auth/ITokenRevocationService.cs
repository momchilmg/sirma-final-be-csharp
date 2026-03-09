namespace FootballPairs.Application.Auth;

public interface ITokenRevocationService
{
    Task RevokeAsync(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken);
    Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken);
}
