namespace FootballPairs.Application.Logging.Models;

public sealed record RequestLogEntry(
    DateTime Date,
    int ErrorCode,
    string? Username,
    string Data);
