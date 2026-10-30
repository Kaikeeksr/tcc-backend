using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.Attendance;
using AttendanceManagement.Application.Tests.Fakes;
using AttendanceManagement.Domain.Enums;
using Xunit;

namespace AttendanceManagement.Application.Tests.Attendance;

public class AttendanceServiceTests
{
    private readonly FakeAttendanceSessionRepository sessions = new();
    private readonly FakeAttendanceRecordRepository records = new();
    private readonly FakeEventLogRepository eventLog = new();
    private readonly AttendanceService service;

    public AttendanceServiceTests()
    {
        service = new AttendanceService(sessions, records, eventLog, new InMemoryUnitOfWork());
    }

    [Fact]
    public async Task OpenAsync_creates_one_present_record_per_active_enrollment()
    {
        var transporterId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var studentA = Guid.NewGuid();
        var studentB = Guid.NewGuid();

        sessions.Groups[(transporterId, groupId)] = ("Van 1", null, null);
        sessions.Rosters[groupId] = [new RosterEntry(studentA, "Aluno A", null), new RosterEntry(studentB, "Aluno B", null)];

        var result = await service.OpenAsync(
            transporterId, groupId, createdBy,
            new OpenAttendanceSessionRequest(SessionType.ToSchool, DateOnly.FromDateTime(DateTime.UtcNow), null, null));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Records.Count);
        Assert.All(result.Value.Records, r => Assert.Equal(AttendanceStatus.Present, r.Status));
    }

    [Fact]
    public async Task OpenAsync_for_same_group_date_and_direction_conflicts()
    {
        var transporterId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var sessionDate = DateOnly.FromDateTime(DateTime.UtcNow);

        sessions.Groups[(transporterId, groupId)] = ("Van 1", null, null);
        sessions.Rosters[groupId] = [];
        var request = new OpenAttendanceSessionRequest(SessionType.ToSchool, sessionDate, null, null);

        var first = await service.OpenAsync(transporterId, groupId, Guid.NewGuid(), request);
        Assert.True(first.IsSuccess);

        var second = await service.OpenAsync(transporterId, groupId, Guid.NewGuid(), request);

        Assert.True(second.IsFailure);
        Assert.Equal("AttendanceSession.AlreadyOpen", second.Error.Code);
    }

    [Fact]
    public async Task OpenAsync_with_another_transporters_id_returns_not_found()
    {
        var transporterId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        sessions.Groups[(transporterId, groupId)] = ("Van 1", null, null);

        var otherTransporterId = Guid.NewGuid();
        var result = await service.OpenAsync(
            otherTransporterId, groupId, Guid.NewGuid(),
            new OpenAttendanceSessionRequest(SessionType.ToSchool, DateOnly.FromDateTime(DateTime.UtcNow), null, null));

        Assert.True(result.IsFailure);
        Assert.Equal("AttendanceSession.GroupNotFound", result.Error.Code);
    }

    [Fact]
    public async Task MarkPickedUpByGuardianAsync_without_can_pickup_is_rejected()
    {
        var transporterId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var guardianId = Guid.NewGuid();
        var session = OpenSessionDirectly(transporterId);
        records.Add(MarkDirectly(session.Id, studentId, transporterId));
        // guardianId deliberadamente ausente de records.GuardiansWhoCanPickup.

        var result = await service.MarkPickedUpByGuardianAsync(
            transporterId, session.Id, studentId, Guid.NewGuid(),
            new MarkPickedUpByGuardianRequest(guardianId, null));

        Assert.True(result.IsFailure);
        Assert.Equal("AttendanceRecord.GuardianCannotPickup", result.Error.Code);
    }

    [Fact]
    public async Task MarkPickedUpByGuardianAsync_with_can_pickup_marks_record_and_logs_event()
    {
        var transporterId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var guardianId = Guid.NewGuid();
        var session = OpenSessionDirectly(transporterId);
        records.Add(MarkDirectly(session.Id, studentId, transporterId));
        records.GuardiansWhoCanPickup.Add((guardianId, studentId));

        var result = await service.MarkPickedUpByGuardianAsync(
            transporterId, session.Id, studentId, Guid.NewGuid(),
            new MarkPickedUpByGuardianRequest(guardianId, "Buscado pela mãe"));

        Assert.True(result.IsSuccess);
        var record = records.Records.Single(r => r.StudentId == studentId);
        Assert.Equal(AttendanceStatus.PickedUpByGuardian, record.Status);
        Assert.Equal(guardianId, record.PickedUpByGuardianId);
        Assert.Contains(eventLog.Entries, e => e.EventType == "AttendanceRecord.PickedUpByGuardian");
    }

    private Domain.Entities.AttendanceSession OpenSessionDirectly(Guid transporterId)
    {
        var session = Domain.Entities.AttendanceSession.Open(
            transporterId, Guid.NewGuid(), SessionType.ToSchool, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid(), null, null).Value;
        sessions.Add(session);
        return session;
    }

    private static Domain.Entities.AttendanceRecord MarkDirectly(Guid sessionId, Guid studentId, Guid transporterId) =>
        Domain.Entities.AttendanceRecord.Mark(sessionId, studentId, transporterId, AttendanceStatus.Present, Guid.NewGuid(), null).Value;
}
