using System.Text;

namespace AttendanceManagement.Infrastructure.Authentication;

/// <summary>
/// Parâmetros do JWT, da seção <c>Jwt</c>. A chave de assinatura NÃO fica no
/// appsettings (é segredo): entra por user-secrets em dev e variável de ambiente
/// em produção. Pública porque a Infrastructure assina e a Api valida.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    // 256 bits = 32 bytes: mínimo para HMAC-SHA256.
    private const int MinKeyBytes = 32;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string SigningKey { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 120;

    /// <summary>Falha no boot com mensagem clara quando algo essencial está faltando.</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SigningKey))
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey não configurada. Rode: " +
                "dotnet user-secrets set \"Jwt:SigningKey\" \"<chave aleatória de 32+ caracteres>\" " +
                "--project src/AttendanceManagement.Api");
        }

        if (Encoding.UTF8.GetByteCount(SigningKey) < MinKeyBytes)
        {
            throw new InvalidOperationException(
                $"Jwt:SigningKey precisa de ao menos {MinKeyBytes} bytes (256 bits) para HMAC-SHA256.");
        }

        if (string.IsNullOrWhiteSpace(Issuer))
        {
            throw new InvalidOperationException("Jwt:Issuer não configurado.");
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException("Jwt:Audience não configurado.");
        }

        if (ExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("Jwt:ExpirationMinutes precisa ser maior que zero.");
        }
    }
}
