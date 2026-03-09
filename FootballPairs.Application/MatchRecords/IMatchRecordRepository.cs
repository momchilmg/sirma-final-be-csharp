using FootballPairs.Domain.Entities;

namespace FootballPairs.Application.MatchRecords;

public interface IMatchRecordRepository
{
    Task<List<MatchRecord>> ListAsync(int? matchId, CancellationToken cancellationToken);
    Task<List<MatchRecord>> ListByPlayersAsync(int playerAId, int playerBId, int? matchId, CancellationToken cancellationToken);
    Task<MatchRecord?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<MatchRecord?> GetByIdReadOnlyAsync(int id, CancellationToken cancellationToken);
    Task<List<MatchRecord>> ListByMatchAndPlayerAsync(int matchId, int playerId, int? excludedId, CancellationToken cancellationToken);
    Task AddAsync(MatchRecord matchRecord, CancellationToken cancellationToken);
    Task DeleteAsync(MatchRecord matchRecord, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
