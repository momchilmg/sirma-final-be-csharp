using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using FootballPairs.Application.Import.Models;
using FootballPairs.Application.MatchRecords;
using FootballPairs.Application.MatchRecords.Models;
using FootballPairs.Application.Matches;
using FootballPairs.Application.Matches.Models;
using FootballPairs.Application.Players;
using FootballPairs.Application.Players.Models;
using FootballPairs.Application.Teams;
using FootballPairs.Application.Teams.Models;

namespace FootballPairs.Application.Import;

public sealed class ImportService(
    ICsvParser csvParser,
    IHeaderMappingService headerMappingService,
    IDateParser dateParser,
    IImportTransaction importTransaction,
    ITeamService teamService,
    IPlayerService playerService,
    IMatchService matchService,
    IMatchRecordService matchRecordService) : IImportService
{
    private const int ImportFlushBatchSize = 200;

    public async Task<ImportSummaryDto> ImportTeamsAsync(Stream sourceStream, CancellationToken cancellationToken)
    {
        var createdCount = await ImportRowsAsync(
            sourceStream,
            TeamRequiredAliases,
            (row, headerMap, rowIndex) =>
            {
                var name = ReadRequiredValue(row, headerMap, "Name", rowIndex);
                var managerFullName = ReadRequiredValue(row, headerMap, "ManagerFullName", rowIndex);
                var group = ReadRequiredValue(row, headerMap, "Group", rowIndex);
                return new CreateTeamCommand(name, managerFullName, group);
            },
            (command, token) => teamService.QueueCreateForImportAsync(command, token),
            teamService.FlushImportAsync,
            ImportFlushBatchSize,
            cancellationToken);

        return new ImportSummaryDto("teams", createdCount);
    }

    public async Task<ImportSummaryDto> ImportPlayersAsync(Stream sourceStream, CancellationToken cancellationToken)
    {
        var createdCount = await ImportRowsAsync(
            sourceStream,
            PlayerRequiredAliases,
            (row, headerMap, rowIndex) =>
            {
                var fullName = ReadRequiredValue(row, headerMap, "FullName", rowIndex);
                var position = ReadRequiredValue(row, headerMap, "Position", rowIndex);
                var teamNumber = ReadRequiredInt(row, headerMap, "TeamNumber", rowIndex);
                var teamId = ReadRequiredInt(row, headerMap, "TeamId", rowIndex);
                return new CreatePlayerCommand(teamNumber, position, fullName, teamId);
            },
            (command, token) => playerService.QueueCreateForImportAsync(command, token),
            playerService.FlushImportAsync,
            ImportFlushBatchSize,
            cancellationToken);

        return new ImportSummaryDto("players", createdCount);
    }

    public async Task<ImportSummaryDto> ImportMatchesAsync(Stream sourceStream, CancellationToken cancellationToken)
    {
        var createdCount = await ImportRowsAsync(
            sourceStream,
            MatchRequiredAliases,
            (row, headerMap, rowIndex) =>
            {
                var homeTeamId = ReadRequiredInt(row, headerMap, "HomeTeamId", rowIndex);
                var awayTeamId = ReadRequiredInt(row, headerMap, "AwayTeamId", rowIndex);
                var dateRaw = ReadRequiredValue(row, headerMap, "MatchDate", rowIndex);
                if (!dateParser.TryParse(dateRaw, out var matchDate))
                {
                    throw new ValidationException($"Row {rowIndex + 2}, column MatchDate: value '{dateRaw}' is not a valid date.");
                }

                var score = ReadRequiredValue(row, headerMap, "Score", rowIndex);
                return new CreateMatchCommand(matchDate, homeTeamId, awayTeamId, score);
            },
            (command, token) => matchService.QueueCreateForImportAsync(command, token),
            matchService.FlushImportAsync,
            ImportFlushBatchSize,
            cancellationToken);

        return new ImportSummaryDto("matches", createdCount);
    }

    public async Task<ImportSummaryDto> ImportMatchRecordsAsync(Stream sourceStream, CancellationToken cancellationToken)
    {
        var createdCount = await ImportRowsAsync(
            sourceStream,
            MatchRecordRequiredAliases,
            (row, headerMap, rowIndex) =>
            {
                var matchId = ReadRequiredInt(row, headerMap, "MatchId", rowIndex);
                var playerId = ReadRequiredInt(row, headerMap, "PlayerId", rowIndex);
                var fromMinute = ReadRequiredInt(row, headerMap, "FromMinute", rowIndex);
                var toMinute = ReadNullableMinute(row, headerMap, rowIndex);
                return new CreateMatchRecordCommand(matchId, playerId, fromMinute, toMinute);
            },
            (command, token) => matchRecordService.QueueCreateForImportAsync(command, token),
            matchRecordService.FlushImportAsync,
            ImportFlushBatchSize,
            cancellationToken);

        return new ImportSummaryDto("match-records", createdCount);
    }

    private async Task<int> ImportRowsAsync<TCommand>(
        Stream sourceStream,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> requiredAliases,
        Func<IReadOnlyList<string>, IReadOnlyDictionary<string, int>, int, TCommand> mapRow,
        Func<TCommand, CancellationToken, Task> queueCommandAsync,
        Func<CancellationToken, Task> flushAsync,
        int flushBatchSize,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(sourceStream, Encoding.UTF8, true, 4096, leaveOpen: true);
        await using var rowEnumerator = csvParser
            .ParseAsync(reader, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);

        try
        {
            if (!await rowEnumerator.MoveNextAsync())
            {
                throw new ValidationException("CSV content is empty.");
            }

            var headers = rowEnumerator.Current;
            if (headers.Count == 0)
            {
                throw new ValidationException("CSV header row is missing.");
            }

            var headerMap = headerMappingService.BuildRequiredMap(headers, requiredAliases);
            var rowIndex = 0;
            var persistedCount = 0;
            var queuedSinceLastFlush = 0;
            await importTransaction.ExecuteAsync(
                async () =>
                {
                    while (await rowEnumerator.MoveNextAsync())
                    {
                        var row = rowEnumerator.Current;
                        if (IsEmptyRow(row))
                        {
                            rowIndex++;
                            continue;
                        }

                        var command = mapRow(row, headerMap, rowIndex);
                        await queueCommandAsync(command, cancellationToken);
                        queuedSinceLastFlush++;
                        if (queuedSinceLastFlush >= flushBatchSize)
                        {
                            await flushAsync(cancellationToken);
                            queuedSinceLastFlush = 0;
                        }

                        persistedCount++;
                        rowIndex++;
                    }

                    if (queuedSinceLastFlush > 0)
                    {
                        await flushAsync(cancellationToken);
                    }
                },
                cancellationToken);

            return persistedCount;
        }
        catch (FormatException exception)
        {
            throw new ValidationException(exception.Message);
        }
    }

    private static bool IsEmptyRow(IReadOnlyList<string> row)
    {
        return row.All(static value => string.IsNullOrWhiteSpace(value));
    }

    private static string ReadRequiredValue(
        IReadOnlyList<string> row,
        IReadOnlyDictionary<string, int> headerMap,
        string columnName,
        int rowIndex)
    {
        if (!headerMap.TryGetValue(columnName, out var index))
        {
            throw new ValidationException($"Required column '{columnName}' is missing.");
        }

        var value = index < row.Count ? row[index]?.Trim() ?? string.Empty : string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"Row {rowIndex + 2}, column {columnName}: value is required.");
        }

        return value;
    }

    private static int ReadRequiredInt(
        IReadOnlyList<string> row,
        IReadOnlyDictionary<string, int> headerMap,
        string columnName,
        int rowIndex)
    {
        var rawValue = ReadRequiredValue(row, headerMap, columnName, rowIndex);
        if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            throw new ValidationException($"Row {rowIndex + 2}, column {columnName}: value '{rawValue}' is not a valid integer.");
        }

        return value;
    }

    private static int? ReadNullableMinute(
        IReadOnlyList<string> row,
        IReadOnlyDictionary<string, int> headerMap,
        int rowIndex)
    {
        if (!headerMap.TryGetValue("ToMinute", out var index))
        {
            throw new ValidationException("Required column 'ToMinute' is missing.");
        }

        var rawValue = index < row.Count ? row[index]?.Trim() ?? string.Empty : string.Empty;
        if (string.IsNullOrWhiteSpace(rawValue) || string.Equals(rawValue, "NULL", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minute))
        {
            throw new ValidationException($"Row {rowIndex + 2}, column ToMinute: value '{rawValue}' is not a valid integer or NULL.");
        }

        return minute;
    }

    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> TeamRequiredAliases =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Name"] = new[] { "Name", "TeamName" },
            ["ManagerFullName"] = new[] { "ManagerFullName", "Manager", "ManagerName" },
            ["Group"] = new[] { "Group", "Grp" }
        };

    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> PlayerRequiredAliases =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["FullName"] = new[] { "FullName", "Name" },
            ["Position"] = new[] { "Position" },
            ["TeamNumber"] = new[] { "TeamNumber", "Number" },
            ["TeamId"] = new[] { "TeamId", "TeamID" }
        };

    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> MatchRequiredAliases =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["HomeTeamId"] = new[] { "ATeamID", "HomeTeamId" },
            ["AwayTeamId"] = new[] { "BTeamID", "AwayTeamId" },
            ["MatchDate"] = new[] { "Date", "MatchDate" },
            ["Score"] = new[] { "Score" }
        };

    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> MatchRecordRequiredAliases =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["PlayerId"] = new[] { "PlayerID", "PlayerId" },
            ["MatchId"] = new[] { "MatchID", "MatchId" },
            ["FromMinute"] = new[] { "fromMinutes", "FromMinute", "FromMinutes" },
            ["ToMinute"] = new[] { "toMinutes", "ToMinute", "ToMinutes" }
        };
}
