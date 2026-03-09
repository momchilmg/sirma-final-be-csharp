using FootballPairs.Domain.Entities;

namespace FootballPairs.Application.Teams;

public interface ITeamRepository
{
    Task<List<Team>> ListAsync(CancellationToken cancellationToken);
    Task<Team?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Team?> GetByIdReadOnlyAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(Team team, CancellationToken cancellationToken);
    Task DeleteAsync(Team team, CancellationToken cancellationToken);
    Task<bool> HasPlayersAsync(int teamId, CancellationToken cancellationToken);
    Task<bool> HasMatchesAsync(int teamId, CancellationToken cancellationToken);
    Task<bool> TeamExistsAsync(int teamId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
