using System.Security.Cryptography;
using FootballPairs.Application.Auth;

namespace FootballPairs.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int DefaultIterations = 100000;

    public (byte[] Hash, byte[] Salt, int Iterations) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, DefaultIterations, HashAlgorithmName.SHA256, HashSize);
        return (hash, salt, DefaultIterations);
    }

    public bool VerifyPassword(string password, byte[] expectedHash, byte[] salt, int iterations)
    {
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
