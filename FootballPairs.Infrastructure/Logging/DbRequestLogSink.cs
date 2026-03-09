using FootballPairs.Application.Logging;
using FootballPairs.Application.Logging.Models;
using FootballPairs.Domain.Entities;
using FootballPairs.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;

namespace FootballPairs.Infrastructure.Logging;

public sealed class DbRequestLogSink(FootballPairsDbContext dbContext) : IPrimaryRequestLogSink
{
    public async Task WriteAsync(RequestLogEntry entry, CancellationToken cancellationToken)
    {
        try
        {
            var requestLog = new RequestLog
            {
                Date = entry.Date,
                ErrorCode = entry.ErrorCode,
                Username = entry.Username,
                Data = entry.Data
            };

            await dbContext.RequestLogs.AddAsync(requestLog, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (IsDatabaseUnavailable(exception))
        {
            throw new PrimaryLogSinkUnavailableException("Primary request log sink is unavailable.", exception);
        }
    }

    private static bool IsDatabaseUnavailable(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is TimeoutException)
            {
                return true;
            }

            if (current is SqlException sqlException && IsAvailabilitySqlError(sqlException.Number))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAvailabilitySqlError(int errorNumber)
    {
        return errorNumber is -2 or 2 or 53 or 64 or 233 or 4060 or 10053 or 10054 or 10060 or 10928 or 10929;
    }
}
