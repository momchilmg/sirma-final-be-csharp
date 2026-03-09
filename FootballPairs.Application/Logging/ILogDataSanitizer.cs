namespace FootballPairs.Application.Logging;

public interface ILogDataSanitizer
{
    string Sanitize(string jsonPayload);
}
