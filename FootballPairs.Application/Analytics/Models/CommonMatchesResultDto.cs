namespace FootballPairs.Application.Analytics.Models;

public sealed record CommonMatchesResultDto(
    IReadOnlyList<CommonMatchItemDto> Matches,
    int TotalMinutesTogether);
