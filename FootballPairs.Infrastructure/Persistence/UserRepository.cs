using System.Data;
using FootballPairs.Application.Auth;
using FootballPairs.Application.Common.Errors;
using FootballPairs.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FootballPairs.Infrastructure.Persistence;

public sealed class UserRepository(FootballPairsDbContext dbContext) : IUserRepository
{
    public Task<bool> AnyUsersAsync(CancellationToken cancellationToken)
    {
        return dbContext.Users.AnyAsync(cancellationToken);
    }

    public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken)
    {
        var normalizedUsername = NormalizeUsername(username);
        return dbContext.Users.AnyAsync(user => user.Username.ToUpper() == normalizedUsername, cancellationToken);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var normalizedUsername = NormalizeUsername(username);
        return dbContext.Users.SingleOrDefaultAsync(user => user.Username.ToUpper() == normalizedUsername, cancellationToken);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        return dbContext.Users.AddAsync(user, cancellationToken).AsTask();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw new ConflictException("Username is already taken.");
        }
    }

    public async Task ExecuteInSerializableTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await action(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException && (sqlException.Number == 2601 || sqlException.Number == 2627);
    }

    private static string NormalizeUsername(string username)
    {
        return username.Trim().ToUpperInvariant();
    }
}
