using System.Security.Cryptography;
using AttendanceManagement.Application.Abstractions;

namespace AttendanceManagement.Infrastructure.Authentication;

/// <summary>
/// Hash de senha com PBKDF2-HMAC-SHA256 (só BCL). Formato gravado:
/// <c>{iterações}.{sal_base64}.{hash_base64}</c> — o número de iterações viaja
/// junto, então dá para endurecer o custo sem invalidar quem já se cadastrou.
/// </summary>
internal sealed class PasswordHasher : IPasswordHasher
{
    // Recomendação do OWASP (2023) para PBKDF2-HMAC-SHA256.
    private const int Iterations = 600_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return string.Join('.', Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool Verify(string password, string passwordHash)
    {
        ArgumentNullException.ThrowIfNull(password);

        if (string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        // Hash malformado é credencial que não confere, não exceção.
        var parts = passwordHash.Split('.');

        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        byte[] salt;
        byte[] expected;

        try
        {
            salt = Convert.FromBase64String(parts[1]);
            expected = Convert.FromBase64String(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
