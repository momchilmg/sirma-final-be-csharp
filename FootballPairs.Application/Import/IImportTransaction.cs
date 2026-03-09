namespace FootballPairs.Application.Import;

public interface IImportTransaction
{
    Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken);
}
