using FootballPairs.Domain.Entities;

namespace FootballPairs.Application.Matches;

public interface IMatchRepository
{
    Task<List<Match>> ListAsync(CancellationToken cancellationToken);
    Task<List<Match>> ListByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);
    Task<bool> ExistsTeamDailyConflictAsync(
        DateTime matchDate,
        int homeTeamId,
        int awayTeamId,
        int? excludedMatchId,
        CancellationToken cancellationToken);
    Task<Match?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Match?> GetByIdReadOnlyAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(Match match, CancellationToken cancellationToken);
    Task DeleteAsync(Match match, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
