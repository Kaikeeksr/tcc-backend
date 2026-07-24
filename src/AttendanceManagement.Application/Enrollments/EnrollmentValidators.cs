using FluentValidation;

namespace AttendanceManagement.Application.Enrollments;

public sealed class EnrollStudentRequestValidator : AbstractValidator<EnrollStudentRequest>
{
    public EnrollStudentRequestValidator() =>
        RuleFor(x => x.TransportGroupId).NotEmpty();
}
