namespace FootballPairs.Application.Teams.Models;

public sealed record CreateTeamCommand(string Name, string ManagerFullName, string Group);
