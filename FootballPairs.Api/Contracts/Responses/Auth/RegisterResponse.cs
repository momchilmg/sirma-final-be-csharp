namespace FootballPairs.Api.Contracts.Responses.Auth;

public sealed record RegisterResponse(Guid Id, string Username, string Role, DateTime CreatedAt);
