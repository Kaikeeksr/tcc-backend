using AttendanceManagement.Domain.Entities;
using FluentValidation;

namespace AttendanceManagement.Application.Authentication;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class RegisterTransporterRequestValidator : AbstractValidator<RegisterTransporterRequest>
{
    public RegisterTransporterRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Transporter.NameMaxLength);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(UserAccount.EmailMaxLength);
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.DocumentType).IsInEnum();
        RuleFor(x => x.DocumentNumber).NotEmpty();
    }
}
