using FootballPairs.Application.Players.Models;

namespace FootballPairs.Application.Players;

public interface IPlayerService
{
    Task<PlayerDto> CreateAsync(CreatePlayerCommand command, CancellationToken cancellationToken);
    Task QueueCreateForImportAsync(CreatePlayerCommand command, CancellationToken cancellationToken);
    Task FlushImportAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<PlayerDto>> ListAsync(CancellationToken cancellationToken);
    Task<PlayerDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<PlayerDto> UpdateAsync(int id, UpdatePlayerCommand command, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
