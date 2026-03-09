using System.Text;
using System.Runtime.CompilerServices;
using FootballPairs.Application.Import;

namespace FootballPairs.Infrastructure.Csv;

public sealed class CsvParser : ICsvParser
{
    public IReadOnlyList<IReadOnlyList<string>> Parse(string content)
    {
        var rows = new List<IReadOnlyList<string>>();
        var currentRow = new List<string>();
        var currentValue = new StringBuilder();
        var isInQuotes = false;
        var currentValueWasQuoted = false;

        void AppendCurrentValue()
        {
            var value = currentValue.ToString();
            currentRow.Add(currentValueWasQuoted ? value : value.Trim());
            currentValue.Clear();
            currentValueWasQuoted = false;
        }

        for (var index = 0; index < content.Length; index++)
        {
            var currentChar = content[index];
            if (isInQuotes)
            {
                if (currentChar == '"')
                {
                    if (index + 1 < content.Length && content[index + 1] == '"')
                    {
                        currentValue.Append('"');
                        index++;
                    }
                    else
                    {
                        isInQuotes = false;
                    }
                }
                else
                {
                    currentValue.Append(currentChar);
                }

                continue;
            }

            if (currentChar == '"')
            {
                if (string.IsNullOrWhiteSpace(currentValue.ToString()))
                {
                    currentValue.Clear();
                }

                currentValueWasQuoted = true;
                isInQuotes = true;
                continue;
            }

            if (currentChar == ',')
            {
                AppendCurrentValue();
                continue;
            }

            if (currentChar == '\r')
            {
                continue;
            }

            if (currentChar == '\n')
            {
                AppendCurrentValue();
                rows.Add(currentRow.ToArray());
                currentRow = [];
                continue;
            }

            currentValue.Append(currentChar);
        }

        if (isInQuotes)
        {
            throw new FormatException("CSV contains an unterminated quoted field.");
        }

        if (currentValue.Length > 0 || currentRow.Count > 0)
        {
            AppendCurrentValue();
            rows.Add(currentRow.ToArray());
        }

        return rows;
    }

    public async IAsyncEnumerable<IReadOnlyList<string>> ParseAsync(
        TextReader reader,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var currentRow = new List<string>();
        var currentValue = new StringBuilder();
        var isInQuotes = false;
        var currentValueWasQuoted = false;

        void AppendCurrentValue()
        {
            var value = currentValue.ToString();
            currentRow.Add(currentValueWasQuoted ? value : value.Trim());
            currentValue.Clear();
            currentValueWasQuoted = false;
        }

        var buffer = new char[4096];
        while (true)
        {
            var charsRead = await reader.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (charsRead == 0)
            {
                break;
            }

            for (var index = 0; index < charsRead; index++)
            {
                var currentChar = buffer[index];
                if (isInQuotes)
                {
                    if (currentChar == '"')
                    {
                        var hasEscapedQuote = false;
                        if (index + 1 < charsRead && buffer[index + 1] == '"')
                        {
                            hasEscapedQuote = true;
                            index++;
                        }
                        else if (index + 1 == charsRead && reader.Peek() == '"')
                        {
                            var consumedChars = await reader.ReadAsync(buffer.AsMemory(0, 1), cancellationToken);
                            hasEscapedQuote = consumedChars == 1 && buffer[0] == '"';
                        }

                        if (hasEscapedQuote)
                        {
                            currentValue.Append('"');
                        }
                        else
                        {
                            isInQuotes = false;
                        }
                    }
                    else
                    {
                        currentValue.Append(currentChar);
                    }

                    continue;
                }

                if (currentChar == '"')
                {
                    if (string.IsNullOrWhiteSpace(currentValue.ToString()))
                    {
                        currentValue.Clear();
                    }

                    currentValueWasQuoted = true;
                    isInQuotes = true;
                    continue;
                }

                if (currentChar == ',')
                {
                    AppendCurrentValue();
                    continue;
                }

                if (currentChar == '\r')
                {
                    continue;
                }

                if (currentChar == '\n')
                {
                    AppendCurrentValue();
                    yield return currentRow.ToArray();
                    currentRow.Clear();
                    continue;
                }

                currentValue.Append(currentChar);
            }
        }

        if (isInQuotes)
        {
            throw new FormatException("CSV contains an unterminated quoted field.");
        }

        if (currentValue.Length > 0 || currentRow.Count > 0)
        {
            AppendCurrentValue();
            yield return currentRow.ToArray();
        }
    }
}
