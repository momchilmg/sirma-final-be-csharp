using FootballPairs.Application.Common.Errors;
using FootballPairs.Application.Matches;
using FootballPairs.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FootballPairs.Infrastructure.Persistence;

public sealed class MatchRepository(FootballPairsDbContext dbContext) : IMatchRepository
{
    private const int SqlServerForeignKeyViolation = 547;

    public Task<List<Match>> ListAsync(CancellationToken cancellationToken)
    {
        return dbContext.Matches
            .AsNoTracking()
            .OrderByDescending(match => match.MatchDate)
            .ThenBy(match => match.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Match>> ListByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return Task.FromResult(new List<Match>());
        }

        return dbContext.Matches
            .AsNoTracking()
            .Where(match => ids.Contains(match.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsTeamDailyConflictAsync(
        DateTime matchDate,
        int homeTeamId,
        int awayTeamId,
        int? excludedMatchId,
        CancellationToken cancellationToken)
    {
        var dayStart = matchDate.Date;
        var dayEnd = dayStart.AddDays(1);

        var persistedExists = await dbContext.Matches
            .AsNoTracking()
            .Where(match => match.MatchDate >= dayStart && match.MatchDate < dayEnd)
            .Where(match =>
                match.HomeTeamId == homeTeamId
                || match.AwayTeamId == homeTeamId
                || match.HomeTeamId == awayTeamId
                || match.AwayTeamId == awayTeamId)
            .Where(match => !excludedMatchId.HasValue || match.Id != excludedMatchId.Value)
            .AnyAsync(cancellationToken);
        if (persistedExists)
        {
            return true;
        }

        var pendingExists = dbContext.ChangeTracker
            .Entries<Match>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .Select(entry => entry.Entity)
            .Where(match => !excludedMatchId.HasValue || match.Id != excludedMatchId.Value)
            .Where(match => match.MatchDate >= dayStart && match.MatchDate < dayEnd)
            .Any(match =>
                match.HomeTeamId == homeTeamId
                || match.AwayTeamId == homeTeamId
                || match.HomeTeamId == awayTeamId
                || match.AwayTeamId == awayTeamId);

        return pendingExists;
    }

    public Task<Match?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Matches.SingleOrDefaultAsync(match => match.Id == id, cancellationToken);
    }

    public Task<Match?> GetByIdReadOnlyAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Matches
            .AsNoTracking()
            .SingleOrDefaultAsync(match => match.Id == id, cancellationToken);
    }

    public Task AddAsync(Match match, CancellationToken cancellationToken)
    {
        return dbContext.Matches.AddAsync(match, cancellationToken).AsTask();
    }

    public Task DeleteAsync(Match match, CancellationToken cancellationToken)
    {
        dbContext.Matches.Remove(match);
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
            throw new ConflictException("Match operation conflicts with related data.");
        }
    }

    private static bool IsForeignKeyViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException && sqlException.Number == SqlServerForeignKeyViolation;
    }
}
