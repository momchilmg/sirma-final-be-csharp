namespace FootballPairs.Application.Common.Errors;

public sealed class UnauthenticatedException(string message = "Authentication is required.")
    : ApplicationErrorException(message, ErrorCodes.Unauthenticated);
