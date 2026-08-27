using AttendanceManagement.Domain.Entities;
using FluentValidation;

namespace AttendanceManagement.Application.Students;

public sealed class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Student.NameMaxLength);
        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("The birth date cannot be in the future.");
        RuleFor(x => x.Grade).MaximumLength(Student.GradeMaxLength);
    }
}

public sealed class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
    public UpdateStudentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Student.NameMaxLength);
        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("The birth date cannot be in the future.");
        RuleFor(x => x.Grade).MaximumLength(Student.GradeMaxLength);
    }
}

public sealed class CreateStudentLoginRequestValidator : AbstractValidator<CreateStudentLoginRequest>
{
    public CreateStudentLoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
