using AttendanceManagement.Application.Common;
using AttendanceManagement.Domain.Entities;
using FluentValidation;

namespace AttendanceManagement.Application.Guardians;

public sealed class GuardianContactPayloadValidator : AbstractValidator<GuardianContactPayload>
{
    public GuardianContactPayloadValidator()
    {
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Mobile).MaximumLength(30);
        RuleFor(x => x.Whatsapp).MaximumLength(30);
        RuleFor(x => x.ContactEmail)
            .EmailAddress()
            .MaximumLength(254)
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
    }
}

public sealed class CreateGuardianRequestValidator : AbstractValidator<CreateGuardianRequest>
{
    public CreateGuardianRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Guardian.NameMaxLength);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(UserAccount.EmailMaxLength);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        When(x => x.Contact is not null, () =>
            RuleFor(x => x.Contact!).SetValidator(new GuardianContactPayloadValidator()));
        When(x => x.Address is not null, () =>
            RuleFor(x => x.Address!).SetValidator(new AddressPayloadValidator()));
    }
}

public sealed class UpdateGuardianRequestValidator : AbstractValidator<UpdateGuardianRequest>
{
    public UpdateGuardianRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Guardian.NameMaxLength);
        When(x => x.Contact is not null, () =>
            RuleFor(x => x.Contact!).SetValidator(new GuardianContactPayloadValidator()));
        When(x => x.Address is not null, () =>
            RuleFor(x => x.Address!).SetValidator(new AddressPayloadValidator()));
    }
}
