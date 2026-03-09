namespace FootballPairs.Api.Contracts.Responses.Analytics;

public sealed record CommonMatchesResponse(
    IReadOnlyList<CommonMatchItemResponse> Matches,
    int TotalMinutesTogether);
