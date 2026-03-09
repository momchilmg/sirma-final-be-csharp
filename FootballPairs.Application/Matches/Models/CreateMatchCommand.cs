namespace FootballPairs.Application.Matches.Models;

public sealed record CreateMatchCommand(DateTime MatchDate, int HomeTeamId, int AwayTeamId, string Score);
