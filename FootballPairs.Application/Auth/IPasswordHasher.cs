namespace FootballPairs.Application.Auth;

public interface IPasswordHasher
{
    (byte[] Hash, byte[] Salt, int Iterations) HashPassword(string password);
    bool VerifyPassword(string password, byte[] expectedHash, byte[] salt, int iterations);
}
