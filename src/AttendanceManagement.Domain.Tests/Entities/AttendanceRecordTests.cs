using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Domain.Enums;
using Xunit;

namespace AttendanceManagement.Domain.Tests.Entities;

public class AttendanceRecordTests
{
    [Fact]
    public void Mark_with_valid_data_succeeds()
    {
        var result = AttendanceRecord.Mark(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), AttendanceStatus.Present, Guid.NewGuid(), schoolId: null);

        Assert.True(result.IsSuccess);
        Assert.Equal(AttendanceStatus.Present, result.Value.Status);
    }

    [Theory]
    [InlineData("session")]
    [InlineData("student")]
    [InlineData("transporter")]
    [InlineData("recordedBy")]
    public void Mark_rejects_missing_required_ids(string missing)
    {
        var sessionId = missing == "session" ? Guid.Empty : Guid.NewGuid();
        var studentId = missing == "student" ? Guid.Empty : Guid.NewGuid();
        var transporterId = missing == "transporter" ? Guid.Empty : Guid.NewGuid();
        var recordedBy = missing == "recordedBy" ? Guid.Empty : Guid.NewGuid();

        var result = AttendanceRecord.Mark(sessionId, studentId, transporterId, AttendanceStatus.Present, recordedBy, null);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void MarkPickedUpByGuardian_requires_guardian_id()
    {
        var result = AttendanceRecord.MarkPickedUpByGuardian(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, "motivo", Guid.NewGuid(), null);

        Assert.True(result.IsFailure);
        Assert.Equal("AttendanceRecord.GuardianRequired", result.Error.Code);
    }

    [Fact]
    public void MarkPickedUpByGuardian_sets_status_and_trims_justification()
    {
        var result = AttendanceRecord.MarkPickedUpByGuardian(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "  foi buscado  ", Guid.NewGuid(), null);

        Assert.True(result.IsSuccess);
        Assert.Equal(AttendanceStatus.PickedUpByGuardian, result.Value.Status);
        Assert.Equal("foi buscado", result.Value.Justification);
    }

    [Fact]
    public void Justify_turns_absent_into_justified()
    {
        var record = AttendanceRecord.Mark(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), AttendanceStatus.Absent, Guid.NewGuid(), null).Value;

        record.Justify("atestado medico", Guid.NewGuid());

        Assert.Equal(AttendanceStatus.Justified, record.Status);
        Assert.Equal("atestado medico", record.Justification);
    }

    [Fact]
    public void Justify_does_not_change_status_when_not_absent()
    {
        var record = AttendanceRecord.Mark(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), AttendanceStatus.Present, Guid.NewGuid(), null).Value;

        record.Justify("motivo qualquer", Guid.NewGuid());

        Assert.Equal(AttendanceStatus.Present, record.Status);
    }

    [Fact]
    public void Justify_with_blank_text_stores_null()
    {
        var record = AttendanceRecord.Mark(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), AttendanceStatus.Absent, Guid.NewGuid(), null).Value;

        record.Justify("   ", Guid.NewGuid());

        Assert.Null(record.Justification);
    }

    [Fact]
    public void UpdateStatus_changes_status()
    {
        var record = AttendanceRecord.Mark(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), AttendanceStatus.Present, Guid.NewGuid(), null).Value;

        record.UpdateStatus(AttendanceStatus.Late);

        Assert.Equal(AttendanceStatus.Late, record.Status);
    }
}
