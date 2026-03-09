namespace FootballPairs.Api.Contracts.Responses.Analytics;

public sealed record CommonMatchItemResponse(
    int MatchId,
    DateTime MatchDate,
    int HomeTeamId,
    int AwayTeamId,
    int MinutesTogether);
