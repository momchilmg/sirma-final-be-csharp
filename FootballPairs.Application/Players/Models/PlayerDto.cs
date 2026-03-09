namespace FootballPairs.Application.Players.Models;

public sealed record PlayerDto(int Id, int TeamNumber, string Position, string FullName, int TeamId);
