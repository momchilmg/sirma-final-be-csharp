using FootballPairs.Domain.Entities;

namespace FootballPairs.Application.Players;

public interface IPlayerRepository
{
    Task<List<Player>> ListAsync(CancellationToken cancellationToken);
    Task<Player?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Player?> GetByIdReadOnlyAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(Player player, CancellationToken cancellationToken);
    Task DeleteAsync(Player player, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
