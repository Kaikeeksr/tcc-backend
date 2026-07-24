using System.Text.Json.Serialization;
using AttendanceManagement.Application.Common;

namespace AttendanceManagement.Application.Guardians;

/// <summary>Cadastra o responsável e sua conta de login na mesma operação.</summary>
public sealed record CreateGuardianRequest(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("password")] string? Password,
    [property: JsonPropertyName("contact")] GuardianContactPayload? Contact,
    [property: JsonPropertyName("address")] AddressPayload? Address);

public sealed record UpdateGuardianRequest(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("contact")] GuardianContactPayload? Contact,
    [property: JsonPropertyName("address")] AddressPayload? Address);

/// <summary>Contatos do responsável (distintos do e-mail de login).</summary>
public sealed record GuardianContactPayload(
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("mobile")] string? Mobile,
    [property: JsonPropertyName("whatsapp")] string? Whatsapp,
    [property: JsonPropertyName("contact_email")] string? ContactEmail);

public sealed record GuardianResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("user_account_id")] Guid UserAccountId,
    [property: JsonPropertyName("contact")] GuardianContactPayload Contact,
    [property: JsonPropertyName("address")] AddressPayload Address);
