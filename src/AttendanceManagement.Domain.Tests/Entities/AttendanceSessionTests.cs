using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Domain.Enums;
using Xunit;

namespace AttendanceManagement.Domain.Tests.Entities;

public class AttendanceSessionTests
{
    private static Result<AttendanceSession> OpenValid() =>
        AttendanceSession.Open(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SessionType.ToSchool,
            DateOnly.FromDateTime(DateTime.UtcNow),
            Guid.NewGuid(),
            vehicleId: null,
            assistantId: null);

    [Fact]
    public void Open_with_valid_data_succeeds_and_starts_open()
    {
        var result = OpenValid();

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsOpen);
        Assert.Equal(SessionStatus.Open, result.Value.Status);
    }

    [Fact]
    public void Open_requires_transporter()
    {
        var result = AttendanceSession.Open(
            Guid.Empty, Guid.NewGuid(), SessionType.ToSchool, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid(), null, null);

        Assert.True(result.IsFailure);
        Assert.Equal("AttendanceSession.TransporterRequired", result.Error.Code);
    }

    [Fact]
    public void Open_requires_transport_group()
    {
        var result = AttendanceSession.Open(
            Guid.NewGuid(), Guid.Empty, SessionType.ToSchool, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid(), null, null);

        Assert.True(result.IsFailure);
        Assert.Equal("AttendanceSession.GroupRequired", result.Error.Code);
    }

    [Fact]
    public void Open_requires_author()
    {
        var result = AttendanceSession.Open(
            Guid.NewGuid(), Guid.NewGuid(), SessionType.ToSchool, DateOnly.FromDateTime(DateTime.UtcNow), Guid.Empty, null, null);

        Assert.True(result.IsFailure);
        Assert.Equal("AttendanceSession.CreatedByRequired", result.Error.Code);
    }

    [Fact]
    public void Close_open_session_succeeds()
    {
        var session = OpenValid().Value;

        var result = session.Close();

        Assert.True(result.IsSuccess);
        Assert.Equal(SessionStatus.Closed, session.Status);
        Assert.NotNull(session.ClosedAtUtc);
    }

    [Fact]
    public void Close_already_closed_session_fails()
    {
        var session = OpenValid().Value;
        session.Close();

        var result = session.Close();

        Assert.True(result.IsFailure);
        Assert.Equal("AttendanceSession.AlreadyClosed", result.Error.Code);
    }

    [Fact]
    public void Close_canceled_session_fails()
    {
        var session = OpenValid().Value;
        session.Cancel();

        var result = session.Close();

        Assert.True(result.IsFailure);
        Assert.Equal("AttendanceSession.Canceled", result.Error.Code);
    }

    [Fact]
    public void Cancel_closed_session_fails()
    {
        var session = OpenValid().Value;
        session.Close();

        var result = session.Cancel();

        Assert.True(result.IsFailure);
        Assert.Equal("AttendanceSession.AlreadyClosed", result.Error.Code);
    }
}
