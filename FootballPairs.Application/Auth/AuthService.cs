using FootballPairs.Application.Auth.Models;
using FootballPairs.Application.Common.Errors;
using FootballPairs.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace FootballPairs.Application.Auth;

public sealed class AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService) : IAuthService
{
    private const string AdminRole = "admin";
    private const string UserRole = "user";

    public async Task<RegisteredUserDto> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var username = command.Username.Trim();
        var password = command.Password;
        ValidateInput(username, password);

        RegisteredUserDto? result = null;
        await userRepository.ExecuteInSerializableTransactionAsync(
            async token =>
            {
                if (await userRepository.UsernameExistsAsync(username, token))
                {
                    throw new ConflictException("Username is already taken.");
                }

                var isFirstUser = !await userRepository.AnyUsersAsync(token);
                var now = DateTime.UtcNow;
                var (hash, salt, iterations) = passwordHasher.HashPassword(password);
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = username,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Iterations = iterations,
                    Role = isFirstUser ? AdminRole : UserRole,
                    CreatedAt = now,
                    LastLoginAt = null
                };

                await userRepository.AddAsync(user, token);
                await userRepository.SaveChangesAsync(token);
                result = new RegisteredUserDto(user.Id, user.Username, user.Role, user.CreatedAt);
            },
            cancellationToken);

        return result ?? throw new InvalidOperationException("User registration failed.");
    }

    public async Task<LoginResultDto> LoginAsync(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var username = command.Username.Trim();
        var password = command.Password;
        ValidateInput(username, password);
        var user = await userRepository.GetByUsernameAsync(username, cancellationToken);
        if (user is null)
        {
            throw new UnauthenticatedException("Invalid username or password.");
        }

        var validPassword = passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt, user.Iterations);
        if (!validPassword)
        {
            throw new UnauthenticatedException("Invalid username or password.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await userRepository.SaveChangesAsync(cancellationToken);
        var accessToken = jwtTokenService.CreateAccessToken(user);
        return new LoginResultDto(accessToken);
    }

    private static void ValidateInput(string username, string password)
    {
        if (username.Length is < 3 or > 64)
        {
            throw new ValidationException("Username must be between 3 and 64 characters.");
        }

        if (password.Length is < 8 or > 128)
        {
            throw new ValidationException("Password must be between 8 and 128 characters.");
        }
    }
}
