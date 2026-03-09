namespace FootballPairs.Application.Common.Errors;

public sealed class ForbiddenException(string message = "You do not have access to this resource.")
    : ApplicationErrorException(message, ErrorCodes.Forbidden);
