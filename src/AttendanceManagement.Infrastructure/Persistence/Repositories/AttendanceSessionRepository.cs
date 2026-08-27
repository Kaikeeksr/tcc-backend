using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.Attendance;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Persistence.Repositories;

internal sealed class AttendanceSessionRepository(AppDbContext context) : IAttendanceSessionRepository
{
    public void Add(AttendanceSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        context.AttendanceSessions.Add(session);
    }

    public Task<AttendanceSession?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        context.AttendanceSessions
            .AsTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.TransporterId == transporterId, cancellationToken);

    public Task<bool> ExistsAsync(Guid transportGroupId, DateOnly sessionDate, SessionType sessionType, CancellationToken cancellationToken = default) =>
        context.AttendanceSessions.AnyAsync(
            s => s.TransportGroupId == transportGroupId && s.SessionDate == sessionDate && s.SessionType == sessionType,
            cancellationToken);

    public async Task<(string Name, Guid? VehicleId, Guid? AssistantId)?> GetGroupInfoAsync(
        Guid transporterId,
        Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var group = await context.TransportGroups
            .Where(g => g.Id == groupId && g.TransporterId == transporterId)
            .Select(g => new { g.Name, g.VehicleId, g.AssistantId })
            .FirstOrDefaultAsync(cancellationToken);

        return group is null ? null : (group.Name, group.VehicleId, group.AssistantId);
    }

    public async Task<IReadOnlyList<RosterEntry>> GetActiveRosterAsync(Guid transportGroupId, CancellationToken cancellationToken = default) =>
        await context.Enrollments
            .Where(e => e.TransportGroupId == transportGroupId && e.Active)
            .OrderBy(e => e.Student.Name)
            .Select(e => new RosterEntry(e.StudentId, e.Student.Name, e.Student.SchoolId))
            .ToListAsync(cancellationToken);

    public async Task<AttendanceSessionResponse?> GetDetailAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default)
    {
        var session = await context.AttendanceSessions
            .Where(s => s.Id == id && s.TransporterId == transporterId)
            .Select(s => new { s.Id, s.TransportGroupId, s.TransportGroup.Name, s.SessionType, s.SessionDate, s.Status, s.VehicleId, s.AssistantId, s.OpenedAtUtc, s.ClosedAtUtc })
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
        {
            return null;
        }

        var recordList = await GetRecordsAsync(id, cancellationToken);

        return new AttendanceSessionResponse(
            session.Id, session.TransportGroupId, session.Name, session.SessionType, session.SessionDate,
            session.Status, session.VehicleId, session.AssistantId, session.OpenedAtUtc, session.ClosedAtUtc, recordList);
    }

    public async Task<AttendanceSessionResponse?> GetDetailByGroupAndDateAsync(
        Guid transporterId,
        Guid transportGroupId,
        DateOnly sessionDate,
        SessionType sessionType,
        CancellationToken cancellationToken = default)
    {
        var session = await context.AttendanceSessions
            .Where(s => s.TransporterId == transporterId && s.TransportGroupId == transportGroupId
                && s.SessionDate == sessionDate && s.SessionType == sessionType)
            .Select(s => new { s.Id, s.TransportGroupId, s.TransportGroup.Name, s.SessionType, s.SessionDate, s.Status, s.VehicleId, s.AssistantId, s.OpenedAtUtc, s.ClosedAtUtc })
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
        {
            return null;
        }

        var recordList = await GetRecordsAsync(session.Id, cancellationToken);

        return new AttendanceSessionResponse(
            session.Id, session.TransportGroupId, session.Name, session.SessionType, session.SessionDate,
            session.Status, session.VehicleId, session.AssistantId, session.OpenedAtUtc, session.ClosedAtUtc, recordList);
    }

    public async Task<IReadOnlyList<AttendanceSessionSummary>> ListByGroupAsync(
        Guid transporterId,
        Guid transportGroupId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default) =>
        await context.AttendanceSessions
            .Where(s => s.TransporterId == transporterId && s.TransportGroupId == transportGroupId
                && s.SessionDate >= from && s.SessionDate <= to)
            .OrderByDescending(s => s.SessionDate)
            .Select(s => new AttendanceSessionSummary(s.Id, s.SessionDate, s.SessionType, s.Status))
            .ToListAsync(cancellationToken);

    private async Task<IReadOnlyList<AttendanceRecordResponse>> GetRecordsAsync(Guid sessionId, CancellationToken cancellationToken) =>
        await context.AttendanceRecords
            .Where(r => r.AttendanceSessionId == sessionId)
            .OrderBy(r => r.Student.Name)
            .Select(r => new AttendanceRecordResponse(
                r.Id,
                r.StudentId,
                r.Student.Name,
                r.Status,
                r.PickedUpByGuardianId,
                r.PickedUpByGuardian == null ? null : r.PickedUpByGuardian.Name,
                r.Justification,
                r.JustifiedBy,
                r.SchoolId,
                r.RecordedAtUtc))
            .ToListAsync(cancellationToken);
}
