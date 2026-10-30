using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.Attendance;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Tests.Fakes;

public sealed class FakeAttendanceRecordRepository : IAttendanceRecordRepository
{
    public List<AttendanceRecord> Records { get; } = [];

    /// <summary>Pares (responsável, aluno) autorizados a retirar, para <see cref="CanGuardianPickupAsync"/>.</summary>
    public HashSet<(Guid GuardianId, Guid StudentId)> GuardiansWhoCanPickup { get; } = [];

    public void Add(AttendanceRecord record) => Records.Add(record);

    public Task<AttendanceRecord?> GetForUpdateAsync(Guid transporterId, Guid sessionId, Guid studentId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Records.FirstOrDefault(r =>
            r.AttendanceSessionId == sessionId && r.StudentId == studentId && r.TransporterId == transporterId));

    public Task<IReadOnlyList<AttendanceRecord>> GetForUpdateBySessionAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AttendanceRecord>>(Records.Where(r => r.AttendanceSessionId == sessionId).ToList());

    public Task<bool> StudentExistsAsync(Guid transporterId, Guid studentId, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not exercised by these tests.");

    public Task<bool> CanGuardianPickupAsync(Guid guardianId, Guid studentId, CancellationToken cancellationToken = default) =>
        Task.FromResult(GuardiansWhoCanPickup.Contains((guardianId, studentId)));

    public Task<TransportGroupAttendanceReport?> GetGroupReportAsync(
        Guid transporterId, Guid transportGroupId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not exercised by these tests.");

    public Task<StudentAttendanceHistory?> GetStudentHistoryAsync(
        Guid transporterId, Guid studentId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not exercised by these tests.");
}
