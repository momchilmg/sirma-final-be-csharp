namespace FootballPairs.Application.Teams.Models;

public sealed record UpdateTeamCommand(string Name, string ManagerFullName, string Group);
