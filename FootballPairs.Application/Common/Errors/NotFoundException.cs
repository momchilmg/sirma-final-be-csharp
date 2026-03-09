namespace FootballPairs.Application.Common.Errors;

public sealed class NotFoundException(string message = "The requested resource was not found.")
    : ApplicationErrorException(message, ErrorCodes.NotFound);
