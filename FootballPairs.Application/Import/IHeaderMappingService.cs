namespace FootballPairs.Application.Import;

public interface IHeaderMappingService
{
    IReadOnlyDictionary<string, int> BuildRequiredMap(
        IReadOnlyList<string> headers,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> requiredAliases);
}
