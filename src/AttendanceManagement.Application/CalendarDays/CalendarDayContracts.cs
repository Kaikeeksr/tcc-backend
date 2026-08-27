using System.Text.Json.Serialization;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Application.CalendarDays;

public sealed record SetCalendarDayRequest(
    [property: JsonPropertyName("date")] DateOnly Date,
    [property: JsonPropertyName("type")] CalendarDayType Type,
    [property: JsonPropertyName("description")] string? Description);

public sealed record CalendarDayResponse(
    [property: JsonPropertyName("date")] DateOnly Date,
    [property: JsonPropertyName("type")] CalendarDayType Type,
    [property: JsonPropertyName("description")] string? Description);
