namespace FootballPairs.Application.Common.Errors;

public abstract class ApplicationErrorException(string message, string errorCode) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}
