using FootballPairs.Application.Matches.Models;

namespace FootballPairs.Application.Matches;

public interface IMatchService
{
    Task<MatchDto> CreateAsync(CreateMatchCommand command, CancellationToken cancellationToken);
    Task QueueCreateForImportAsync(CreateMatchCommand command, CancellationToken cancellationToken);
    Task FlushImportAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<MatchDto>> ListAsync(CancellationToken cancellationToken);
    Task<MatchDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<MatchDto> UpdateAsync(int id, UpdateMatchCommand command, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
