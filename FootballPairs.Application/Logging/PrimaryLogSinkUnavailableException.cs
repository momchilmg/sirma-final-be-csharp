namespace FootballPairs.Application.Logging;

public sealed class PrimaryLogSinkUnavailableException(string message, Exception innerException) : Exception(message, innerException)
{
}
