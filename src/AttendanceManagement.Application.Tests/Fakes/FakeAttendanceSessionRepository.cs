using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.Attendance;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Application.Tests.Fakes;

public sealed class FakeAttendanceSessionRepository : IAttendanceSessionRepository
{
    public List<AttendanceSession> Sessions { get; } = [];

    /// <summary>Grupos conhecidos (por transportador), para <see cref="GetGroupInfoAsync"/>.</summary>
    public Dictionary<(Guid TransporterId, Guid GroupId), (string Name, Guid? VehicleId, Guid? AssistantId)> Groups { get; } = [];

    /// <summary>Roster matriculado de cada grupo, para <see cref="GetActiveRosterAsync"/>.</summary>
    public Dictionary<Guid, List<RosterEntry>> Rosters { get; } = [];

    public void Add(AttendanceSession session) => Sessions.Add(session);

    public Task<AttendanceSession?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Sessions.FirstOrDefault(s => s.Id == id && s.TransporterId == transporterId));

    public Task<bool> ExistsAsync(Guid transportGroupId, DateOnly sessionDate, SessionType sessionType, CancellationToken cancellationToken = default) =>
        Task.FromResult(Sessions.Any(s =>
            s.TransportGroupId == transportGroupId && s.SessionDate == sessionDate && s.SessionType == sessionType));

    public Task<(string Name, Guid? VehicleId, Guid? AssistantId)?> GetGroupInfoAsync(Guid transporterId, Guid groupId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Groups.TryGetValue((transporterId, groupId), out var info)
            ? info
            : ((string Name, Guid? VehicleId, Guid? AssistantId)?)null);

    public Task<IReadOnlyList<RosterEntry>> GetActiveRosterAsync(Guid transportGroupId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RosterEntry>>(Rosters.TryGetValue(transportGroupId, out var roster) ? roster : []);

    public Task<AttendanceSessionResponse?> GetDetailAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not exercised by these tests.");

    public Task<AttendanceSessionResponse?> GetDetailByGroupAndDateAsync(
        Guid transporterId, Guid transportGroupId, DateOnly sessionDate, SessionType sessionType, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not exercised by these tests.");

    public Task<IReadOnlyList<AttendanceSessionSummary>> ListByGroupAsync(
        Guid transporterId, Guid transportGroupId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not exercised by these tests.");
}
