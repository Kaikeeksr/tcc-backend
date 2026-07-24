using System.Text.Json.Serialization;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Application.Transporters;

/// <summary>Envelope do payload: <c>{ "transport_team": { ... } }</c>.</summary>
public sealed record TransportTeamEnvelope(
    [property: JsonPropertyName("transport_team")] TransportTeam Team);

/// <summary>Read model da equipe de um transportador: ele e os grupos que opera. Projetado direto no SELECT.</summary>
public sealed record TransportTeam(
    [property: JsonPropertyName("driver")] string Driver,
    [property: JsonPropertyName("groups")] IReadOnlyList<TransportGroupSummary> Groups);

/// <summary>Um grupo e a equipe designada a ele. Monitor e veículo são opcionais (o grupo existe antes da designação).</summary>
public sealed record TransportGroupSummary(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("shift")] string? Shift,
    [property: JsonPropertyName("assistant")] AssistantSummary? Assistant,
    [property: JsonPropertyName("vehicle")] VehicleSummary? Vehicle,
    [property: JsonPropertyName("students")] IReadOnlyList<StudentSummary> Students)
{
    [JsonPropertyName("student_count")]
    public int StudentCount => Students.Count;
}

/// <summary>Um aluno matriculado no grupo. A escola pende do aluno, não do grupo.</summary>
public sealed record StudentSummary(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("grade")] string? Grade,
    [property: JsonPropertyName("school")] SchoolSummary? School,
    [property: JsonPropertyName("guardians")] IReadOnlyList<GuardianSummary> Guardians);

/// <summary>Um responsável ativo de um aluno, com o que a tela da chamada precisa.</summary>
public sealed record GuardianSummary(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("relationship")] RelationshipType Relationship,
    [property: JsonPropertyName("is_primary")] bool IsPrimary,
    [property: JsonPropertyName("can_pickup")] bool CanPickup,
    [property: JsonPropertyName("mobile")] string? Mobile,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("whatsapp")] string? Whatsapp);

public sealed record SchoolSummary(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name);

public sealed record AssistantSummary(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name);

public sealed record VehicleSummary(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("plate")] string Plate,
    [property: JsonPropertyName("model")] string? Model);
