using FluentValidation;

namespace AttendanceManagement.Application.GuardianStudents;

public sealed class LinkGuardianRequestValidator : AbstractValidator<LinkGuardianRequest>
{
    public LinkGuardianRequestValidator()
    {
        RuleFor(x => x.GuardianId).NotEmpty();
        RuleFor(x => x.Relationship).IsInEnum();
    }
}

public sealed class UpdateGuardianStudentRequestValidator : AbstractValidator<UpdateGuardianStudentRequest>
{
    public UpdateGuardianStudentRequestValidator() =>
        RuleFor(x => x.Relationship).IsInEnum();
}
