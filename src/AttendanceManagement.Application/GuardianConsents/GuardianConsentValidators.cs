using FluentValidation;

namespace AttendanceManagement.Application.GuardianConsents;

public sealed class GrantGuardianConsentRequestValidator : AbstractValidator<GrantGuardianConsentRequest>
{
    public GrantGuardianConsentRequestValidator() =>
        RuleFor(x => x.TermsVersion).NotEmpty();
}
