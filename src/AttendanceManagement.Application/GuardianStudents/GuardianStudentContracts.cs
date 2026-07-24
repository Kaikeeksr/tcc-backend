using System.Text.Json.Serialization;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Application.GuardianStudents;

public sealed record LinkGuardianRequest(
    [property: JsonPropertyName("guardian_id")] Guid GuardianId,
    [property: JsonPropertyName("relationship")] RelationshipType Relationship,
    [property: JsonPropertyName("is_primary")] bool IsPrimary,
    [property: JsonPropertyName("can_pickup")] bool CanPickup);

public sealed record UpdateGuardianStudentRequest(
    [property: JsonPropertyName("relationship")] RelationshipType Relationship,
    [property: JsonPropertyName("is_primary")] bool IsPrimary,
    [property: JsonPropertyName("can_pickup")] bool CanPickup);

public sealed record GuardianStudentResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("guardian_id")] Guid GuardianId,
    [property: JsonPropertyName("student_id")] Guid StudentId,
    [property: JsonPropertyName("relationship")] RelationshipType Relationship,
    [property: JsonPropertyName("is_primary")] bool IsPrimary,
    [property: JsonPropertyName("can_pickup")] bool CanPickup,
    [property: JsonPropertyName("active")] bool Active);
