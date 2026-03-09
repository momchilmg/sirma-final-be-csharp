using FootballPairs.Application.Auth;
using FootballPairs.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FootballPairs.Infrastructure.Persistence;

public sealed class RevokedTokenRepository(FootballPairsDbContext dbContext) : IRevokedTokenRepository
{
    public Task<bool> ExistsAsync(string jti, CancellationToken cancellationToken)
    {
        return dbContext.RevokedTokens.AnyAsync(token => token.Jti == jti, cancellationToken);
    }

    public Task AddAsync(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken)
    {
        return dbContext.RevokedTokens.AddAsync(
            new RevokedToken
            {
                Jti = jti,
                ExpiresAtUtc = expiresAtUtc,
                RevokedAtUtc = DateTime.UtcNow
            },
            cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return SaveChangesInternalAsync(cancellationToken);
    }

    private async Task SaveChangesInternalAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            // Two concurrent logout requests for the same token are treated as idempotent.
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException && (sqlException.Number == 2601 || sqlException.Number == 2627);
    }
}
