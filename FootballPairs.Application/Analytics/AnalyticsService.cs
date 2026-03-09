using System.ComponentModel.DataAnnotations;
using FootballPairs.Application.Analytics.Models;
using FootballPairs.Application.Common.Errors;
using FootballPairs.Application.MatchRecords;
using FootballPairs.Application.Matches;
using FootballPairs.Application.Players;
using FootballPairs.Domain.Entities;

namespace FootballPairs.Application.Analytics;

public sealed class AnalyticsService(
    IPlayerRepository playerRepository,
    IMatchRepository matchRepository,
    IMatchRecordRepository matchRecordRepository) : IAnalyticsService
{
    public async Task<PlayedTimeResultDto> GetPlayedTimeTogetherAsync(
        int playerAId,
        int playerBId,
        DateTime? fromDate,
        DateTime? toDate,
        int? matchId,
        CancellationToken cancellationToken)
    {
        ValidateInput(playerAId, playerBId, fromDate, toDate, matchId);
        await EnsurePlayersExistAsync(playerAId, playerBId, cancellationToken);
        if (matchId.HasValue && await matchRepository.GetByIdReadOnlyAsync(matchId.Value, cancellationToken) is null)
        {
            throw new NotFoundException("Match was not found.");
        }

        var commonMatches = await BuildCommonMatchesAsync(playerAId, playerBId, fromDate, toDate, matchId, cancellationToken);
        return new PlayedTimeResultDto(commonMatches.Sum(match => match.MinutesTogether));
    }

    public async Task<CommonMatchesResultDto> GetCommonMatchesAsync(
        int playerAId,
        int playerBId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken)
    {
        ValidateInput(playerAId, playerBId, fromDate, toDate, null);
        await EnsurePlayersExistAsync(playerAId, playerBId, cancellationToken);
        var commonMatches = await BuildCommonMatchesAsync(playerAId, playerBId, fromDate, toDate, null, cancellationToken);
        return new CommonMatchesResultDto(commonMatches, commonMatches.Sum(match => match.MinutesTogether));
    }

    private async Task<IReadOnlyList<CommonMatchItemDto>> BuildCommonMatchesAsync(
        int playerAId,
        int playerBId,
        DateTime? fromDate,
        DateTime? toDate,
        int? matchId,
        CancellationToken cancellationToken)
    {
        var records = await matchRecordRepository.ListByPlayersAsync(playerAId, playerBId, matchId, cancellationToken);
        if (records.Count == 0)
        {
            return [];
        }

        var matchIds = records.Select(record => record.MatchId).Distinct().ToArray();
        var matches = await matchRepository.ListByIdsAsync(matchIds, cancellationToken);
        var matchesById = matches.ToDictionary(match => match.Id);
        var results = new List<CommonMatchItemDto>();
        foreach (var matchRecords in records.GroupBy(record => record.MatchId))
        {
            if (!matchesById.TryGetValue(matchRecords.Key, out var match))
            {
                continue;
            }

            if (!IsInDateRange(match.MatchDate, fromDate, toDate))
            {
                continue;
            }

            var playerARecords = matchRecords.Where(record => record.PlayerId == playerAId).ToArray();
            var playerBRecords = matchRecords.Where(record => record.PlayerId == playerBId).ToArray();
            if (playerARecords.Length == 0 || playerBRecords.Length == 0)
            {
                continue;
            }

            var minutesTogether = AnalyticsOverlap.ComputeTotalOverlap(playerARecords, playerBRecords, match.EndMinute);
            results.Add(new CommonMatchItemDto(match.Id, match.MatchDate, match.HomeTeamId, match.AwayTeamId, minutesTogether));
        }

        return results
            .OrderBy(match => match.MatchDate)
            .ThenBy(match => match.MatchId)
            .ToArray();
    }

    private async Task EnsurePlayersExistAsync(int playerAId, int playerBId, CancellationToken cancellationToken)
    {
        if (await playerRepository.GetByIdReadOnlyAsync(playerAId, cancellationToken) is null)
        {
            throw new NotFoundException("Player A was not found.");
        }

        if (await playerRepository.GetByIdReadOnlyAsync(playerBId, cancellationToken) is null)
        {
            throw new NotFoundException("Player B was not found.");
        }
    }

    private static bool IsInDateRange(DateTime matchDate, DateTime? fromDate, DateTime? toDate)
    {
        var date = matchDate.Date;
        if (fromDate.HasValue && date < fromDate.Value.Date)
        {
            return false;
        }

        if (toDate.HasValue && date > toDate.Value.Date)
        {
            return false;
        }

        return true;
    }

    private static void ValidateInput(int playerAId, int playerBId, DateTime? fromDate, DateTime? toDate, int? matchId)
    {
        if (playerAId <= 0 || playerBId <= 0)
        {
            throw new ValidationException("Player ids must be positive.");
        }

        if (playerAId == playerBId)
        {
            throw new ValidationException("Player ids must be different.");
        }

        if (matchId.HasValue && matchId.Value <= 0)
        {
            throw new ValidationException("Match id must be positive.");
        }

        if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
        {
            throw new ValidationException("From date must be less than or equal to to date.");
        }
    }
}
