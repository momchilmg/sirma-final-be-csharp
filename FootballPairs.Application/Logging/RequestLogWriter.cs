using FootballPairs.Application.Logging.Models;

namespace FootballPairs.Application.Logging;

public sealed class RequestLogWriter(IPrimaryRequestLogSink primarySink, IFallbackRequestLogSink fallbackSink) : IRequestLogWriter
{
    public async Task WriteAsync(RequestLogEntry entry, CancellationToken cancellationToken)
    {
        try
        {
            await primarySink.WriteAsync(entry, cancellationToken);
        }
        catch (PrimaryLogSinkUnavailableException)
        {
            await fallbackSink.WriteAsync(entry, CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            await fallbackSink.WriteAsync(entry, CancellationToken.None);
        }
    }
}
