namespace FootballPairs.Application.Analytics.Models;

public sealed record CommonMatchItemDto(
    int MatchId,
    DateTime MatchDate,
    int HomeTeamId,
    int AwayTeamId,
    int MinutesTogether);
