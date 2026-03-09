using FootballPairs.Application.Common.Errors;
using FootballPairs.Application.Players;
using FootballPairs.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FootballPairs.Infrastructure.Persistence;

public sealed class PlayerRepository(FootballPairsDbContext dbContext) : IPlayerRepository
{
    private const int SqlServerForeignKeyViolation = 547;

    public Task<List<Player>> ListAsync(CancellationToken cancellationToken)
    {
        return dbContext.Players
            .AsNoTracking()
            .OrderBy(player => player.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Player?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Players.SingleOrDefaultAsync(player => player.Id == id, cancellationToken);
    }

    public Task<Player?> GetByIdReadOnlyAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Players
            .AsNoTracking()
            .SingleOrDefaultAsync(player => player.Id == id, cancellationToken);
    }

    public Task AddAsync(Player player, CancellationToken cancellationToken)
    {
        return dbContext.Players.AddAsync(player, cancellationToken).AsTask();
    }

    public Task DeleteAsync(Player player, CancellationToken cancellationToken)
    {
        dbContext.Players.Remove(player);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            throw new ConflictException("Player operation conflicts with related data.");
        }
    }

    private static bool IsForeignKeyViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException && sqlException.Number == SqlServerForeignKeyViolation;
    }
}
