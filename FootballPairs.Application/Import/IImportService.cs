using FootballPairs.Application.Import.Models;

namespace FootballPairs.Application.Import;

public interface IImportService
{
    Task<ImportSummaryDto> ImportTeamsAsync(Stream sourceStream, CancellationToken cancellationToken);
    Task<ImportSummaryDto> ImportPlayersAsync(Stream sourceStream, CancellationToken cancellationToken);
    Task<ImportSummaryDto> ImportMatchesAsync(Stream sourceStream, CancellationToken cancellationToken);
    Task<ImportSummaryDto> ImportMatchRecordsAsync(Stream sourceStream, CancellationToken cancellationToken);
}
