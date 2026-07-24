using System.Text.Json.Serialization;

namespace AttendanceManagement.Application.Enrollments;

public sealed record EnrollStudentRequest(
    [property: JsonPropertyName("transport_group_id")] Guid TransportGroupId);

public sealed record EnrollmentResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("student_id")] Guid StudentId,
    [property: JsonPropertyName("transport_group_id")] Guid TransportGroupId,
    [property: JsonPropertyName("group_name")] string GroupName,
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("started_at_utc")] DateTime StartedAtUtc,
    [property: JsonPropertyName("ended_at_utc")] DateTime? EndedAtUtc);
