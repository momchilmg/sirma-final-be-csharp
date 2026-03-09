using FootballPairs.Application.Teams;
using FootballPairs.Application.Common.Errors;
using FootballPairs.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FootballPairs.Infrastructure.Persistence;

public sealed class TeamRepository(FootballPairsDbContext dbContext) : ITeamRepository
{
    public Task<List<Team>> ListAsync(CancellationToken cancellationToken)
    {
        return dbContext.Teams
            .AsNoTracking()
            .OrderBy(team => team.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Team?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Teams.SingleOrDefaultAsync(team => team.Id == id, cancellationToken);
    }

    public Task<Team?> GetByIdReadOnlyAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Teams
            .AsNoTracking()
            .SingleOrDefaultAsync(team => team.Id == id, cancellationToken);
    }

    public Task AddAsync(Team team, CancellationToken cancellationToken)
    {
        return dbContext.Teams.AddAsync(team, cancellationToken).AsTask();
    }

    public Task DeleteAsync(Team team, CancellationToken cancellationToken)
    {
        dbContext.Teams.Remove(team);
        return Task.CompletedTask;
    }

    public Task<bool> HasPlayersAsync(int teamId, CancellationToken cancellationToken)
    {
        return dbContext.Players.AnyAsync(player => player.TeamId == teamId, cancellationToken);
    }

    public Task<bool> HasMatchesAsync(int teamId, CancellationToken cancellationToken)
    {
        return dbContext.Matches.AnyAsync(
            match => match.HomeTeamId == teamId || match.AwayTeamId == teamId,
            cancellationToken);
    }

    public Task<bool> TeamExistsAsync(int teamId, CancellationToken cancellationToken)
    {
        return dbContext.Teams.AnyAsync(team => team.Id == teamId, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            throw new ConflictException("Team operation conflicts with related data.");
        }
    }

    private static bool IsForeignKeyViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException && sqlException.Number == 547;
    }
}
