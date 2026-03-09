namespace FootballPairs.Api.Contracts.Responses.Matches;

public sealed record MatchResponse(
    int Id,
    DateTime MatchDate,
    int HomeTeamId,
    int AwayTeamId,
    string Score,
    int EndMinute);
