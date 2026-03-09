using FootballPairs.Domain.Entities;

namespace FootballPairs.Application.Auth;

public interface IJwtTokenService
{
    string CreateAccessToken(User user);
}
