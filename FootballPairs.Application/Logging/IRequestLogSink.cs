using FootballPairs.Application.Logging.Models;

namespace FootballPairs.Application.Logging;

public interface IRequestLogSink
{
    Task WriteAsync(RequestLogEntry entry, CancellationToken cancellationToken);
}
