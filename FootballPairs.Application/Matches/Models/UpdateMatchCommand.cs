namespace FootballPairs.Application.Matches.Models;

public sealed record UpdateMatchCommand(DateTime MatchDate, int HomeTeamId, int AwayTeamId, string Score);
