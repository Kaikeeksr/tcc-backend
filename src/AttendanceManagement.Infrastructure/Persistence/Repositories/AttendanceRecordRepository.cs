using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.Attendance;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Persistence.Repositories;

internal sealed class AttendanceRecordRepository(AppDbContext context) : IAttendanceRecordRepository
{
    public void Add(AttendanceRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        context.AttendanceRecords.Add(record);
    }

    public Task<AttendanceRecord?> GetForUpdateAsync(Guid transporterId, Guid sessionId, Guid studentId, CancellationToken cancellationToken = default) =>
        context.AttendanceRecords
            .AsTracking()
            .FirstOrDefaultAsync(
                r => r.TransporterId == transporterId && r.AttendanceSessionId == sessionId && r.StudentId == studentId,
                cancellationToken);

    public async Task<IReadOnlyList<AttendanceRecord>> GetForUpdateBySessionAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        await context.AttendanceRecords
            .AsTracking()
            .Where(r => r.AttendanceSessionId == sessionId)
            .ToListAsync(cancellationToken);

    public Task<bool> StudentExistsAsync(Guid transporterId, Guid studentId, CancellationToken cancellationToken = default) =>
        context.Students.AnyAsync(s => s.Id == studentId && s.TransporterId == transporterId, cancellationToken);

    public Task<bool> CanGuardianPickupAsync(Guid guardianId, Guid studentId, CancellationToken cancellationToken = default) =>
        context.GuardianStudents.AnyAsync(
            gs => gs.GuardianId == guardianId && gs.StudentId == studentId && gs.Active && gs.CanPickup,
            cancellationToken);

    public async Task<TransportGroupAttendanceReport?> GetGroupReportAsync(
        Guid transporterId,
        Guid transportGroupId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var groupName = await context.TransportGroups
            .Where(g => g.Id == transportGroupId && g.TransporterId == transporterId)
            .Select(g => g.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (groupName is null)
        {
            return null;
        }

        var sessionIds = await context.AttendanceSessions
            .Where(s => s.TransportGroupId == transportGroupId && s.TransporterId == transporterId
                && s.SessionDate >= from && s.SessionDate <= to && s.Status != SessionStatus.Canceled)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        var totalSessions = sessionIds.Count;

        var rawStats = await context.AttendanceRecords
            .Where(r => sessionIds.Contains(r.AttendanceSessionId))
            .GroupBy(r => new { r.StudentId, r.Student.Name })
            .Select(g => new
            {
                g.Key.StudentId,
                g.Key.Name,
                Present = g.Count(r => r.Status == AttendanceStatus.Present),
                Absent = g.Count(r => r.Status == AttendanceStatus.Absent),
                Late = g.Count(r => r.Status == AttendanceStatus.Late),
                Picked = g.Count(r => r.Status == AttendanceStatus.PickedUpByGuardian),
                Justified = g.Count(r => r.Status == AttendanceStatus.Justified),
                Total = g.Count(),
            })
            .ToListAsync(cancellationToken);

        var students = rawStats
            .OrderBy(s => s.Name)
            .Select(s => new StudentAttendanceStat(
                s.StudentId,
                s.Name,
                s.Total,
                s.Present,
                s.Absent,
                s.Late,
                s.Picked,
                s.Justified,
                s.Total == 0 ? 0 : Math.Round((s.Present + s.Late + s.Picked) * 100.0 / s.Total, 1)))
            .ToList();

        var average = students.Count == 0 ? 0 : Math.Round(students.Average(s => s.AttendanceRate), 1);

        return new TransportGroupAttendanceReport(transportGroupId, groupName, from, to, totalSessions, students, average);
    }

    public async Task<StudentAttendanceHistory?> GetStudentHistoryAsync(
        Guid transporterId,
        Guid studentId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var studentName = await context.Students
            .Where(s => s.Id == studentId && s.TransporterId == transporterId)
            .Select(s => s.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (studentName is null)
        {
            return null;
        }

        var rows = await context.AttendanceRecords
            .Where(r => r.StudentId == studentId && r.TransporterId == transporterId)
            .Where(r => r.AttendanceSession.SessionDate >= from && r.AttendanceSession.SessionDate <= to)
            .Where(r => r.AttendanceSession.Status != SessionStatus.Canceled)
            .OrderByDescending(r => r.AttendanceSession.SessionDate)
            .Select(r => new
            {
                r.AttendanceSessionId,
                r.AttendanceSession.SessionDate,
                r.AttendanceSession.SessionType,
                r.Status,
                r.Justification,
            })
            .ToListAsync(cancellationToken);

        var present = rows.Count(r => r.Status == AttendanceStatus.Present);
        var absent = rows.Count(r => r.Status == AttendanceStatus.Absent);
        var late = rows.Count(r => r.Status == AttendanceStatus.Late);
        var picked = rows.Count(r => r.Status == AttendanceStatus.PickedUpByGuardian);
        var justified = rows.Count(r => r.Status == AttendanceStatus.Justified);
        var total = rows.Count;
        var rate = total == 0 ? 0 : Math.Round((present + late + picked) * 100.0 / total, 1);

        var history = rows
            .Select(r => new StudentAttendanceHistoryItem(r.AttendanceSessionId, r.SessionDate, r.SessionType, r.Status, r.Justification))
            .ToList();

        return new StudentAttendanceHistory(studentId, studentName, from, to, total, present, absent, late, picked, justified, rate, history);
    }
}
