namespace FootballPairs.Application.Import;

public interface IDateParser
{
    bool TryParse(string rawValue, out DateTime value);
}
