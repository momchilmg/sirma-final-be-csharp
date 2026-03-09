using System.ComponentModel.DataAnnotations;

namespace FootballPairs.Application.Auth;

public sealed class TokenRevocationService(IRevokedTokenRepository revokedTokenRepository) : ITokenRevocationService
{
    public async Task RevokeAsync(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken)
    {
        var normalizedJti = NormalizeJti(jti);
        if (await revokedTokenRepository.ExistsAsync(normalizedJti, cancellationToken))
        {
            return;
        }

        await revokedTokenRepository.AddAsync(normalizedJti, expiresAtUtc, cancellationToken);
        await revokedTokenRepository.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken)
    {
        var normalizedJti = NormalizeJti(jti);
        return revokedTokenRepository.ExistsAsync(normalizedJti, cancellationToken);
    }

    private static string NormalizeJti(string jti)
    {
        var normalizedJti = jti.Trim();
        if (string.IsNullOrWhiteSpace(normalizedJti))
        {
            throw new ValidationException("Token identifier is required.");
        }

        return normalizedJti;
    }
}
