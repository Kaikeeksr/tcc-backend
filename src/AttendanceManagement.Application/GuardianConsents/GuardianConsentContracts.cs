using System.Text.Json.Serialization;

namespace AttendanceManagement.Application.GuardianConsents;

public sealed record GrantGuardianConsentRequest(
    [property: JsonPropertyName("terms_version")] string? TermsVersion);

/// <summary>Estado do consentimento de um filho, na tela "meus filhos" do responsável.</summary>
public sealed record GuardianConsentStatusResponse(
    [property: JsonPropertyName("student_id")] Guid StudentId,
    [property: JsonPropertyName("student_name")] string StudentName,
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("terms_version")] string? TermsVersion,
    [property: JsonPropertyName("granted_at_utc")] DateTime? GrantedAtUtc,
    [property: JsonPropertyName("revoked_at_utc")] DateTime? RevokedAtUtc);
