using FootballPairs.Domain.Entities;

namespace FootballPairs.Application.Auth;

public interface IUserRepository
{
    Task<bool> AnyUsersAsync(CancellationToken cancellationToken);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task ExecuteInSerializableTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken);
}
