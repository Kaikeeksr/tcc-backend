using System.Text.Json.Serialization;

namespace AttendanceManagement.Application.Assistants;

/// <summary>Cadastra o monitor e sua conta de login na mesma operação.</summary>
public sealed record CreateAssistantRequest(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("password")] string? Password);

public sealed record UpdateAssistantRequest(
    [property: JsonPropertyName("name")] string? Name);

public sealed record AssistantResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("user_account_id")] Guid UserAccountId);
