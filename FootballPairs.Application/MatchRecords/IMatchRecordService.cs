using FootballPairs.Application.MatchRecords.Models;

namespace FootballPairs.Application.MatchRecords;

public interface IMatchRecordService
{
    Task<MatchRecordDto> CreateAsync(CreateMatchRecordCommand command, CancellationToken cancellationToken);
    Task QueueCreateForImportAsync(CreateMatchRecordCommand command, CancellationToken cancellationToken);
    Task FlushImportAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<MatchRecordDto>> ListAsync(int? matchId, CancellationToken cancellationToken);
    Task<MatchRecordDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<MatchRecordDto> UpdateAsync(int id, UpdateMatchRecordCommand command, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
