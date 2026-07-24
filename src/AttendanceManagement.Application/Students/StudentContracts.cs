using System.Text.Json.Serialization;

namespace AttendanceManagement.Application.Students;

public sealed record CreateStudentRequest(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("birth_date")] DateOnly BirthDate,
    [property: JsonPropertyName("grade")] string? Grade,
    [property: JsonPropertyName("school_id")] Guid? SchoolId);

public sealed record UpdateStudentRequest(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("birth_date")] DateOnly BirthDate,
    [property: JsonPropertyName("grade")] string? Grade,
    [property: JsonPropertyName("school_id")] Guid? SchoolId);

public sealed record StudentResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("birth_date")] DateOnly BirthDate,
    [property: JsonPropertyName("grade")] string? Grade,
    [property: JsonPropertyName("school_id")] Guid? SchoolId);
