namespace FootballPairs.Application.Matches.Models;

public sealed record MatchDto(
    int Id,
    DateTime MatchDate,
    int HomeTeamId,
    int AwayTeamId,
    string Score,
    int EndMinute);
