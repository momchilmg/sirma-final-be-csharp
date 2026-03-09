namespace FootballPairs.Application.Players.Models;

public sealed record CreatePlayerCommand(int TeamNumber, string Position, string FullName, int TeamId);
