using System.Text.Json.Serialization;

namespace AttendanceManagement.Application.TransportGroups;

public sealed record CreateTransportGroupRequest(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("shift")] string? Shift);

public sealed record UpdateTransportGroupRequest(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("shift")] string? Shift);

/// <summary>Designação da equipe do grupo. Ids nulos desfazem a designação.</summary>
public sealed record AssignCrewRequest(
    [property: JsonPropertyName("vehicle_id")] Guid? VehicleId,
    [property: JsonPropertyName("assistant_id")] Guid? AssistantId);

public sealed record TransportGroupResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("shift")] string? Shift,
    [property: JsonPropertyName("vehicle_id")] Guid? VehicleId,
    [property: JsonPropertyName("assistant_id")] Guid? AssistantId);
