using System.Text.RegularExpressions;

namespace FootballPairs.Application.Matches;

public static partial class MatchScoreValidator
{
    public static bool IsValid(string score)
    {
        return RegularTimeRegex().IsMatch(score) || PenaltyRegex().IsMatch(score);
    }

    [GeneratedRegex(@"^\d{1,2}-\d{1,2}$", RegexOptions.Compiled)]
    private static partial Regex RegularTimeRegex();

    [GeneratedRegex(@"^\d{1,2}\(\d{1,2}\)-\d{1,2}\(\d{1,2}\)$", RegexOptions.Compiled)]
    private static partial Regex PenaltyRegex();
}
