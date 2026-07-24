using AttendanceManagement.Application.Common;
using AttendanceManagement.Domain.Entities;
using FluentValidation;

namespace AttendanceManagement.Application.Schools;

public sealed class CreateSchoolRequestValidator : AbstractValidator<CreateSchoolRequest>
{
    public CreateSchoolRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(School.NameMaxLength);
        When(x => x.Address is not null, () =>
            RuleFor(x => x.Address!).SetValidator(new AddressPayloadValidator()));
    }
}

public sealed class UpdateSchoolRequestValidator : AbstractValidator<UpdateSchoolRequest>
{
    public UpdateSchoolRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(School.NameMaxLength);
        When(x => x.Address is not null, () =>
            RuleFor(x => x.Address!).SetValidator(new AddressPayloadValidator()));
    }
}
