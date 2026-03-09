namespace FootballPairs.Api.Contracts.Responses.Players;

public sealed record PlayerResponse(int Id, int TeamNumber, string Position, string FullName, int TeamId);
