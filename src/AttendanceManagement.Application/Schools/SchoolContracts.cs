using System.Text.Json.Serialization;
using AttendanceManagement.Application.Common;

namespace AttendanceManagement.Application.Schools;

public sealed record CreateSchoolRequest(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("address")] AddressPayload? Address);

public sealed record UpdateSchoolRequest(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("address")] AddressPayload? Address);

public sealed record SchoolResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("address")] AddressPayload Address);
