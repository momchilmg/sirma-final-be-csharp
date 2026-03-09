namespace FootballPairs.Application.Auth.Models;

public sealed record RegisteredUserDto(Guid Id, string Username, string Role, DateTime CreatedAt);
