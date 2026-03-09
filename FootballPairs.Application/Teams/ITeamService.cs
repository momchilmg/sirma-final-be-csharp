using FootballPairs.Application.Teams.Models;

namespace FootballPairs.Application.Teams;

public interface ITeamService
{
    Task<TeamDto> CreateAsync(CreateTeamCommand command, CancellationToken cancellationToken);
    Task QueueCreateForImportAsync(CreateTeamCommand command, CancellationToken cancellationToken);
    Task FlushImportAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<TeamDto>> ListAsync(CancellationToken cancellationToken);
    Task<TeamDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<TeamDto> UpdateAsync(int id, UpdateTeamCommand command, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
