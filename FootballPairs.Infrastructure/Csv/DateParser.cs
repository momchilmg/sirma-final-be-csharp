using System.Globalization;
using FootballPairs.Application.Import;

namespace FootballPairs.Infrastructure.Csv;

public sealed class DateParser : IDateParser
{
    private static readonly string[] Formats =
    [
        "M/d/yyyy",
        "MM/dd/yyyy",
        "yyyy-MM-dd",
        "yyyy/MM/dd",
        "d.M.yyyy",
        "dd.MM.yyyy",
        "M/d/yyyy H:mm:ss",
        "MM/dd/yyyy HH:mm:ss"
    ];

    public bool TryParse(string rawValue, out DateTime value)
    {
        if (DateTime.TryParseExact(
            rawValue.Trim(),
            Formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces,
            out value))
        {
            return true;
        }

        return DateTime.TryParse(rawValue.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out value);
    }
}
