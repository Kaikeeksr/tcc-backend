using AttendanceManagement.Domain.Entities;
using FluentValidation;

namespace AttendanceManagement.Application.Attendance;

public sealed class OpenAttendanceSessionRequestValidator : AbstractValidator<OpenAttendanceSessionRequest>
{
    public OpenAttendanceSessionRequestValidator() =>
        RuleFor(x => x.SessionDate).NotEqual(default(DateOnly));
}

public sealed class MarkAttendanceRecordsRequestValidator : AbstractValidator<MarkAttendanceRecordsRequest>
{
    public MarkAttendanceRecordsRequestValidator()
    {
        RuleFor(x => x.Records).NotEmpty();

        RuleForEach(x => x.Records).ChildRules(record =>
        {
            record.RuleFor(r => r.StudentId).NotEmpty();
        });
    }
}

public sealed class MarkPickedUpByGuardianRequestValidator : AbstractValidator<MarkPickedUpByGuardianRequest>
{
    public MarkPickedUpByGuardianRequestValidator()
    {
        RuleFor(x => x.GuardianId).NotEmpty();
        RuleFor(x => x.Justification).MaximumLength(AttendanceRecord.JustificationMaxLength);
    }
}

public sealed class JustifyAttendanceRecordRequestValidator : AbstractValidator<JustifyAttendanceRecordRequest>
{
    public JustifyAttendanceRecordRequestValidator() =>
        RuleFor(x => x.Justification).MaximumLength(AttendanceRecord.JustificationMaxLength);
}
