using System.Security.Cryptography;
using System.Text;
using HidroRec.Backend.Application.Interfaces;

namespace HidroRec.Backend.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private const string Prefix = "pbkdf2";

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        if (passwordHash.StartsWith($"{Prefix}$", StringComparison.OrdinalIgnoreCase))
        {
            return VerifyPbkdf2(password, passwordHash);
        }

        return VerifyLegacySha256(password, passwordHash);
    }

    private static bool VerifyPbkdf2(string password, string passwordHash)
    {
        var parts = passwordHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[2]);
        var expectedHash = Convert.FromBase64String(parts[3]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static bool VerifyLegacySha256(string password, string passwordHash)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        var computedHash = Convert.ToHexString(bytes);
        return string.Equals(computedHash, passwordHash, StringComparison.OrdinalIgnoreCase);
    }
}
