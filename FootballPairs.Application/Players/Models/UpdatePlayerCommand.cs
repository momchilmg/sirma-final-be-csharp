namespace FootballPairs.Application.Players.Models;

public sealed record UpdatePlayerCommand(int TeamNumber, string Position, string FullName, int TeamId);
