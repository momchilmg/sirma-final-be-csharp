namespace FootballPairs.Application.Common.Errors;

public sealed class ConflictException(string message = "The request conflicts with the current state.")
    : ApplicationErrorException(message, ErrorCodes.Conflict);
