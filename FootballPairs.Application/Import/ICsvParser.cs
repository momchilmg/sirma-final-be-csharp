namespace FootballPairs.Application.Import;

public interface ICsvParser
{
    IReadOnlyList<IReadOnlyList<string>> Parse(string content);
    IAsyncEnumerable<IReadOnlyList<string>> ParseAsync(TextReader reader, CancellationToken cancellationToken);
}
