namespace FootballPairs.Application.Auth;

public interface IRevokedTokenRepository
{
    Task<bool> ExistsAsync(string jti, CancellationToken cancellationToken);
    Task AddAsync(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
