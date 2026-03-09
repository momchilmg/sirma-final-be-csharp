using FootballPairs.Application.Logging.Models;

namespace FootballPairs.Application.Logging;

public interface IRequestLogWriter
{
    Task WriteAsync(RequestLogEntry entry, CancellationToken cancellationToken);
}
