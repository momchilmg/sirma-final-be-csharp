using System.ComponentModel.DataAnnotations;
using FootballPairs.Application.Import;

namespace FootballPairs.Infrastructure.Csv;

public sealed class HeaderMappingService : IHeaderMappingService
{
    public IReadOnlyDictionary<string, int> BuildRequiredMap(
        IReadOnlyList<string> headers,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> requiredAliases)
    {
        var headerIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < headers.Count; index++)
        {
            var header = headers[index].Trim();
            if (string.IsNullOrWhiteSpace(header))
            {
                continue;
            }

            if (!headerIndexes.TryAdd(header, index))
            {
                throw new ValidationException($"CSV contains duplicate header '{header}'.");
            }
        }

        var mapped = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var (canonicalName, aliases) in requiredAliases)
        {
            var matchedIndex = aliases
                .Where(alias => headerIndexes.ContainsKey(alias))
                .Select(alias => headerIndexes[alias])
                .DefaultIfEmpty(-1)
                .First();

            if (matchedIndex < 0)
            {
                throw new ValidationException($"Required column '{canonicalName}' is missing.");
            }

            mapped[canonicalName] = matchedIndex;
        }

        return mapped;
    }
}
