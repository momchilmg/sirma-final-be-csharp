using FootballPairs.Application.Auth.Models;

namespace FootballPairs.Application.Auth;

public interface IAuthService
{
    Task<RegisteredUserDto> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken);
    Task<LoginResultDto> LoginAsync(LoginUserCommand command, CancellationToken cancellationToken);
}
