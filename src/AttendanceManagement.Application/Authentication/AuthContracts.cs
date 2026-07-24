using System.Text.Json.Serialization;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Application.Authentication;

public sealed record LoginRequest(
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("password")] string? Password);

/// <summary>Cadastro do transportador — a raiz do tenant e único papel com auto-cadastro.</summary>
public sealed record RegisterTransporterRequest(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("password")] string? Password,
    [property: JsonPropertyName("document_type")] DocumentType DocumentType,
    [property: JsonPropertyName("document_number")] string? DocumentNumber);

/// <summary>Token e o usuário já pronto para o frontend — os mesmos dados que vão dentro do JWT.</summary>
public sealed record LoginResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_at_utc")] DateTime ExpiresAtUtc,
    [property: JsonPropertyName("user")] AuthenticatedUser User);

public sealed record AuthenticatedUser(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("role")] PrimaryRole Role,
    [property: JsonPropertyName("profile_id")] Guid ProfileId,
    [property: JsonPropertyName("transporter_id")] Guid TransporterId);

/// <summary>
/// Claims que dependem do perfil (nome e tenant), e não da conta de login. Para
/// o transportador, <see cref="ProfileId"/> e <see cref="TransporterId"/> coincidem.
/// </summary>
public sealed record UserIdentity(Guid ProfileId, string DisplayName, Guid TransporterId);

public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);
