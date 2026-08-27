using System.Text.Json.Serialization;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Application.Attendance;

public sealed record OpenAttendanceSessionRequest(
    [property: JsonPropertyName("session_type")] SessionType SessionType,
    [property: JsonPropertyName("session_date")] DateOnly SessionDate,
    [property: JsonPropertyName("vehicle_id")] Guid? VehicleId,
    [property: JsonPropertyName("assistant_id")] Guid? AssistantId);

public sealed record MarkAttendanceRecordItem(
    [property: JsonPropertyName("student_id")] Guid StudentId,
    [property: JsonPropertyName("status")] AttendanceStatus Status);

public sealed record MarkAttendanceRecordsRequest(
    [property: JsonPropertyName("records")] IReadOnlyList<MarkAttendanceRecordItem>? Records);

public sealed record MarkPickedUpByGuardianRequest(
    [property: JsonPropertyName("guardian_id")] Guid GuardianId,
    [property: JsonPropertyName("justification")] string? Justification);

public sealed record JustifyAttendanceRecordRequest(
    [property: JsonPropertyName("justification")] string? Justification,
    [property: JsonPropertyName("justified_by")] Guid? JustifiedBy);

public sealed record AttendanceRecordResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("student_id")] Guid StudentId,
    [property: JsonPropertyName("student_name")] string StudentName,
    [property: JsonPropertyName("status")] AttendanceStatus Status,
    [property: JsonPropertyName("picked_up_by_guardian_id")] Guid? PickedUpByGuardianId,
    [property: JsonPropertyName("picked_up_by_guardian_name")] string? PickedUpByGuardianName,
    [property: JsonPropertyName("justification")] string? Justification,
    [property: JsonPropertyName("justified_by")] Guid? JustifiedBy,
    [property: JsonPropertyName("school_id")] Guid? SchoolId,
    [property: JsonPropertyName("recorded_at_utc")] DateTime RecordedAtUtc);

public sealed record AttendanceSessionResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("transport_group_id")] Guid TransportGroupId,
    [property: JsonPropertyName("transport_group_name")] string TransportGroupName,
    [property: JsonPropertyName("session_type")] SessionType SessionType,
    [property: JsonPropertyName("session_date")] DateOnly SessionDate,
    [property: JsonPropertyName("status")] SessionStatus Status,
    [property: JsonPropertyName("vehicle_id")] Guid? VehicleId,
    [property: JsonPropertyName("assistant_id")] Guid? AssistantId,
    [property: JsonPropertyName("opened_at_utc")] DateTime? OpenedAtUtc,
    [property: JsonPropertyName("closed_at_utc")] DateTime? ClosedAtUtc,
    [property: JsonPropertyName("records")] IReadOnlyList<AttendanceRecordResponse> Records);

public sealed record AttendanceSessionSummary(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("session_date")] DateOnly SessionDate,
    [property: JsonPropertyName("session_type")] SessionType SessionType,
    [property: JsonPropertyName("status")] SessionStatus Status);

public sealed record StudentAttendanceStat(
    [property: JsonPropertyName("student_id")] Guid StudentId,
    [property: JsonPropertyName("student_name")] string StudentName,
    [property: JsonPropertyName("total_sessions")] int TotalSessions,
    [property: JsonPropertyName("present")] int Present,
    [property: JsonPropertyName("absent")] int Absent,
    [property: JsonPropertyName("late")] int Late,
    [property: JsonPropertyName("picked_up_by_guardian")] int PickedUpByGuardian,
    [property: JsonPropertyName("justified")] int Justified,
    [property: JsonPropertyName("attendance_rate")] double AttendanceRate);

public sealed record TransportGroupAttendanceReport(
    [property: JsonPropertyName("transport_group_id")] Guid TransportGroupId,
    [property: JsonPropertyName("transport_group_name")] string TransportGroupName,
    [property: JsonPropertyName("from")] DateOnly From,
    [property: JsonPropertyName("to")] DateOnly To,
    [property: JsonPropertyName("total_sessions")] int TotalSessions,
    [property: JsonPropertyName("students")] IReadOnlyList<StudentAttendanceStat> Students,
    [property: JsonPropertyName("average_attendance_rate")] double AverageAttendanceRate);

public sealed record StudentAttendanceHistoryItem(
    [property: JsonPropertyName("session_id")] Guid SessionId,
    [property: JsonPropertyName("session_date")] DateOnly SessionDate,
    [property: JsonPropertyName("session_type")] SessionType SessionType,
    [property: JsonPropertyName("status")] AttendanceStatus Status,
    [property: JsonPropertyName("justification")] string? Justification);

public sealed record StudentAttendanceHistory(
    [property: JsonPropertyName("student_id")] Guid StudentId,
    [property: JsonPropertyName("student_name")] string StudentName,
    [property: JsonPropertyName("from")] DateOnly From,
    [property: JsonPropertyName("to")] DateOnly To,
    [property: JsonPropertyName("total_sessions")] int TotalSessions,
    [property: JsonPropertyName("present")] int Present,
    [property: JsonPropertyName("absent")] int Absent,
    [property: JsonPropertyName("late")] int Late,
    [property: JsonPropertyName("picked_up_by_guardian")] int PickedUpByGuardian,
    [property: JsonPropertyName("justified")] int Justified,
    [property: JsonPropertyName("attendance_rate")] double AttendanceRate,
    [property: JsonPropertyName("history")] IReadOnlyList<StudentAttendanceHistoryItem> History);
