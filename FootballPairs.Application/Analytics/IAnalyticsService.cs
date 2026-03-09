using FootballPairs.Application.Analytics.Models;

namespace FootballPairs.Application.Analytics;

public interface IAnalyticsService
{
    Task<PlayedTimeResultDto> GetPlayedTimeTogetherAsync(
        int playerAId,
        int playerBId,
        DateTime? fromDate,
        DateTime? toDate,
        int? matchId,
        CancellationToken cancellationToken);

    Task<CommonMatchesResultDto> GetCommonMatchesAsync(
        int playerAId,
        int playerBId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken);
}
