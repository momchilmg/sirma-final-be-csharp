using System.ComponentModel.DataAnnotations;
using FootballPairs.Application.Common.Errors;
using FootballPairs.Application.MatchRecords;
using FootballPairs.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FootballPairs.Infrastructure.Persistence;

public sealed class MatchRecordRepository(FootballPairsDbContext dbContext) : IMatchRecordRepository
{
    private const int SqlServerForeignKeyViolation = 547;
    private const int SqlServerUniqueConstraintViolation = 2627;
    private const int SqlServerUniqueIndexViolation = 2601;
    private const int SqlServerUserDefinedError = 50000;
    private const int SqlServerTriggerOverlapError = 51000;
    private const string TriggerOverlapCode = "MATCH_RECORD_INTERVAL_OVERLAP";

    public Task<List<MatchRecord>> ListAsync(int? matchId, CancellationToken cancellationToken)
    {
        var query = dbContext.MatchRecords.AsNoTracking();
        if (matchId.HasValue)
        {
            query = query.Where(matchRecord => matchRecord.MatchId == matchId.Value);
        }

        return query
            .OrderBy(matchRecord => matchRecord.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<List<MatchRecord>> ListByPlayersAsync(int playerAId, int playerBId, int? matchId, CancellationToken cancellationToken)
    {
        var query = dbContext.MatchRecords
            .AsNoTracking()
            .Where(matchRecord => matchRecord.PlayerId == playerAId || matchRecord.PlayerId == playerBId);
        if (matchId.HasValue)
        {
            query = query.Where(matchRecord => matchRecord.MatchId == matchId.Value);
        }

        return query.ToListAsync(cancellationToken);
    }

    public Task<MatchRecord?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.MatchRecords.SingleOrDefaultAsync(matchRecord => matchRecord.Id == id, cancellationToken);
    }

    public Task<MatchRecord?> GetByIdReadOnlyAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.MatchRecords
            .AsNoTracking()
            .SingleOrDefaultAsync(matchRecord => matchRecord.Id == id, cancellationToken);
    }

    public Task<List<MatchRecord>> ListByMatchAndPlayerAsync(int matchId, int playerId, int? excludedId, CancellationToken cancellationToken)
    {
        return ListByMatchAndPlayerWithPendingAsync(matchId, playerId, excludedId, cancellationToken);
    }

    public Task AddAsync(MatchRecord matchRecord, CancellationToken cancellationToken)
    {
        return dbContext.MatchRecords.AddAsync(matchRecord, cancellationToken).AsTask();
    }

    public Task DeleteAsync(MatchRecord matchRecord, CancellationToken cancellationToken)
    {
        dbContext.MatchRecords.Remove(matchRecord);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsOverlapConflict(exception))
        {
            throw new ConflictException("Match record interval overlaps an existing record.");
        }
        catch (DbUpdateException exception) when (IsForeignKeyViolation(exception))
        {
            throw new ValidationException("Referenced match or player does not exist.");
        }
        catch (DbUpdateException exception) when (IsSingleRecordPerPlayerConflict(exception))
        {
            throw new ConflictException("A player can have only one match record per match.");
        }
    }

    private static bool IsOverlapConflict(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
            && (sqlException.Number == SqlServerTriggerOverlapError || sqlException.Number == SqlServerUserDefinedError)
            && sqlException.Message.Contains(TriggerOverlapCode, StringComparison.Ordinal);
    }

    private static bool IsForeignKeyViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
            && sqlException.Number == SqlServerForeignKeyViolation;
    }

    private static bool IsSingleRecordPerPlayerConflict(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
            && (sqlException.Number == SqlServerUniqueConstraintViolation || sqlException.Number == SqlServerUniqueIndexViolation)
            && sqlException.Message.Contains("IX_MatchRecords_MatchId_PlayerId", StringComparison.Ordinal);
    }

    private async Task<List<MatchRecord>> ListByMatchAndPlayerWithPendingAsync(
        int matchId,
        int playerId,
        int? excludedId,
        CancellationToken cancellationToken)
    {
        var query = dbContext.MatchRecords
            .AsNoTracking()
            .Where(matchRecord => matchRecord.MatchId == matchId && matchRecord.PlayerId == playerId);
        if (excludedId.HasValue)
        {
            query = query.Where(matchRecord => matchRecord.Id != excludedId.Value);
        }

        var persistedRecords = await query.ToListAsync(cancellationToken);
        var pendingRecords = dbContext.ChangeTracker
            .Entries<MatchRecord>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .Select(entry => entry.Entity)
            .Where(matchRecord => matchRecord.MatchId == matchId && matchRecord.PlayerId == playerId)
            .Where(matchRecord => !excludedId.HasValue || matchRecord.Id != excludedId.Value)
            .ToList();
        foreach (var pendingRecord in pendingRecords)
        {
            if (pendingRecord.Id > 0 && persistedRecords.Any(record => record.Id == pendingRecord.Id))
            {
                continue;
            }

            persistedRecords.Add(pendingRecord);
        }

        return persistedRecords;
    }
}
