using System.Text.Json;
using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Application.Attendance;

/// <summary>
/// Caso de uso da chamada: abrir a sessão do dia (com o roster dos alunos
/// matriculados), marcar presença em lote, registrar retirada pelo responsável
/// (validando <c>GuardianStudent.CanPickup</c>), justificar, fechar/cancelar.
/// Escreve no <see cref="EventLog"/> os eventos relevantes.
/// </summary>
public sealed class AttendanceService(
    IAttendanceSessionRepository sessions,
    IAttendanceRecordRepository records,
    IEventLogRepository eventLog,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<AttendanceSessionResponse>> OpenAsync(
        Guid transporterId,
        Guid transportGroupId,
        Guid createdBy,
        OpenAttendanceSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var group = await sessions.GetGroupInfoAsync(transporterId, transportGroupId, cancellationToken);
        if (group is null)
        {
            return Result.Failure<AttendanceSessionResponse>(
                Error.NotFound("AttendanceSession.GroupNotFound", "Transport group not found."));
        }

        if (await sessions.ExistsAsync(transportGroupId, request.SessionDate, request.SessionType, cancellationToken))
        {
            return Result.Failure<AttendanceSessionResponse>(
                Error.Conflict("AttendanceSession.AlreadyOpen", "An attendance session already exists for this group, date and direction."));
        }

        var vehicleId = request.VehicleId ?? group.Value.VehicleId;
        var assistantId = request.AssistantId ?? group.Value.AssistantId;

        var sessionResult = AttendanceSession.Open(
            transporterId,
            transportGroupId,
            request.SessionType,
            request.SessionDate,
            createdBy,
            vehicleId,
            assistantId);

        if (sessionResult.IsFailure)
        {
            return Result.Failure<AttendanceSessionResponse>(sessionResult.Error);
        }

        var session = sessionResult.Value;
        sessions.Add(session);

        var roster = await sessions.GetActiveRosterAsync(transportGroupId, cancellationToken);
        var createdRecords = new List<AttendanceRecordResponse>(roster.Count);

        foreach (var student in roster)
        {
            var recordResult = AttendanceRecord.Mark(
                session.Id,
                student.StudentId,
                transporterId,
                AttendanceStatus.Present,
                createdBy,
                student.SchoolId);

            if (recordResult.IsFailure)
            {
                return Result.Failure<AttendanceSessionResponse>(recordResult.Error);
            }

            var record = recordResult.Value;
            records.Add(record);
            createdRecords.Add(ToRecordResponse(record, student.StudentName, null));
        }

        eventLog.Add(EventLog.Record(
            transporterId,
            "AttendanceSession.Opened",
            nameof(AttendanceSession),
            session.Id,
            createdBy,
            JsonSerializer.Serialize(new { transportGroupId, request.SessionType, request.SessionDate, studentCount = roster.Count })));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new AttendanceSessionResponse(
            session.Id,
            transportGroupId,
            group.Value.Name,
            session.SessionType,
            session.SessionDate,
            session.Status,
            session.VehicleId,
            session.AssistantId,
            session.OpenedAtUtc,
            session.ClosedAtUtc,
            createdRecords));
    }

    public async Task<Result<AttendanceSessionResponse>> GetAsync(
        Guid transporterId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var detail = await sessions.GetDetailAsync(transporterId, id, cancellationToken);

        return detail is null
            ? Result.Failure<AttendanceSessionResponse>(Error.NotFound("AttendanceSession.NotFound", "Attendance session not found."))
            : Result.Success(detail);
    }

    public async Task<Result<AttendanceSessionResponse>> GetByGroupAndDateAsync(
        Guid transporterId,
        Guid transportGroupId,
        DateOnly sessionDate,
        SessionType sessionType,
        CancellationToken cancellationToken = default)
    {
        var detail = await sessions.GetDetailByGroupAndDateAsync(transporterId, transportGroupId, sessionDate, sessionType, cancellationToken);

        return detail is null
            ? Result.Failure<AttendanceSessionResponse>(Error.NotFound("AttendanceSession.NotFound", "No attendance session for this date."))
            : Result.Success(detail);
    }

    public Task<IReadOnlyList<AttendanceSessionSummary>> ListByGroupAsync(
        Guid transporterId,
        Guid transportGroupId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default) =>
        sessions.ListByGroupAsync(transporterId, transportGroupId, from, to, cancellationToken);

    public async Task<Result> MarkRecordsAsync(
        Guid transporterId,
        Guid sessionId,
        Guid actorUserId,
        MarkAttendanceRecordsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var session = await sessions.GetForUpdateAsync(transporterId, sessionId, cancellationToken);
        if (session is null)
        {
            return Result.Failure(Error.NotFound("AttendanceSession.NotFound", "Attendance session not found."));
        }

        if (!session.IsOpen)
        {
            return Result.Failure(Error.Conflict("AttendanceSession.NotOpen", "The attendance session is not open."));
        }

        if (request.Records is null || request.Records.Count == 0)
        {
            return Result.Failure(Error.Validation("AttendanceSession.RecordsRequired", "Provide at least one record."));
        }

        var existing = (await records.GetForUpdateBySessionAsync(sessionId, cancellationToken))
            .ToDictionary(r => r.StudentId);

        foreach (var item in request.Records)
        {
            if (item.Status == AttendanceStatus.PickedUpByGuardian)
            {
                return Result.Failure(Error.Validation(
                    "AttendanceRecord.UsePickupEndpoint",
                    "Use the dedicated pickup endpoint to mark a student as picked up by the guardian."));
            }

            if (!existing.TryGetValue(item.StudentId, out var record))
            {
                return Result.Failure(Error.NotFound("AttendanceRecord.NotFound", "The student is not part of this session's roster."));
            }

            record.UpdateStatus(item.Status);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> MarkPickedUpByGuardianAsync(
        Guid transporterId,
        Guid sessionId,
        Guid studentId,
        Guid actorUserId,
        MarkPickedUpByGuardianRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var session = await sessions.GetForUpdateAsync(transporterId, sessionId, cancellationToken);
        if (session is null)
        {
            return Result.Failure(Error.NotFound("AttendanceSession.NotFound", "Attendance session not found."));
        }

        if (!session.IsOpen)
        {
            return Result.Failure(Error.Conflict("AttendanceSession.NotOpen", "The attendance session is not open."));
        }

        var existing = await records.GetForUpdateAsync(transporterId, sessionId, studentId, cancellationToken);
        if (existing is null)
        {
            return Result.Failure(Error.NotFound("AttendanceRecord.NotFound", "The student is not part of this session's roster."));
        }

        if (!await records.CanGuardianPickupAsync(request.GuardianId, studentId, cancellationToken))
        {
            return Result.Failure(Error.Validation(
                "AttendanceRecord.GuardianCannotPickup",
                "This guardian is not allowed to pick up this student."));
        }

        existing.MarkPickedUp(request.GuardianId, request.Justification);

        eventLog.Add(EventLog.Record(
            transporterId,
            "AttendanceRecord.PickedUpByGuardian",
            nameof(AttendanceRecord),
            existing.Id,
            actorUserId,
            JsonSerializer.Serialize(new { studentId, guardianId = request.GuardianId })));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> JustifyAsync(
        Guid transporterId,
        Guid sessionId,
        Guid studentId,
        JustifyAttendanceRecordRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var record = await records.GetForUpdateAsync(transporterId, sessionId, studentId, cancellationToken);
        if (record is null)
        {
            return Result.Failure(Error.NotFound("AttendanceRecord.NotFound", "The student is not part of this session's roster."));
        }

        record.Justify(request.Justification, request.JustifiedBy);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> CloseAsync(
        Guid transporterId,
        Guid sessionId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var session = await sessions.GetForUpdateAsync(transporterId, sessionId, cancellationToken);
        if (session is null)
        {
            return Result.Failure(Error.NotFound("AttendanceSession.NotFound", "Attendance session not found."));
        }

        var result = session.Close();
        if (result.IsFailure)
        {
            return result;
        }

        eventLog.Add(EventLog.Record(
            transporterId, "AttendanceSession.Closed", nameof(AttendanceSession), session.Id, actorUserId, null));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> CancelAsync(
        Guid transporterId,
        Guid sessionId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var session = await sessions.GetForUpdateAsync(transporterId, sessionId, cancellationToken);
        if (session is null)
        {
            return Result.Failure(Error.NotFound("AttendanceSession.NotFound", "Attendance session not found."));
        }

        var result = session.Cancel();
        if (result.IsFailure)
        {
            return result;
        }

        eventLog.Add(EventLog.Record(
            transporterId, "AttendanceSession.Canceled", nameof(AttendanceSession), session.Id, actorUserId, null));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<TransportGroupAttendanceReport>> GetGroupReportAsync(
        Guid transporterId,
        Guid transportGroupId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var report = await records.GetGroupReportAsync(transporterId, transportGroupId, from, to, cancellationToken);

        return report is null
            ? Result.Failure<TransportGroupAttendanceReport>(Error.NotFound("AttendanceReport.GroupNotFound", "Transport group not found."))
            : Result.Success(report);
    }

    public async Task<Result<StudentAttendanceHistory>> GetStudentHistoryAsync(
        Guid transporterId,
        Guid studentId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var history = await records.GetStudentHistoryAsync(transporterId, studentId, from, to, cancellationToken);

        return history is null
            ? Result.Failure<StudentAttendanceHistory>(Error.NotFound("Student.NotFound", "Student not found."))
            : Result.Success(history);
    }

    private static AttendanceRecordResponse ToRecordResponse(AttendanceRecord record, string studentName, string? guardianName) =>
        new(
            record.Id,
            record.StudentId,
            studentName,
            record.Status,
            record.PickedUpByGuardianId,
            guardianName,
            record.Justification,
            record.JustifiedBy,
            record.SchoolId,
            record.RecordedAtUtc);
}
