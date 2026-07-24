using AttendanceManagement.Domain.Entities;
using FluentValidation;

namespace AttendanceManagement.Application.TransportGroups;

public sealed class CreateTransportGroupRequestValidator : AbstractValidator<CreateTransportGroupRequest>
{
    public CreateTransportGroupRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(TransportGroup.NameMaxLength);
        RuleFor(x => x.Shift).MaximumLength(40);
    }
}

public sealed class UpdateTransportGroupRequestValidator : AbstractValidator<UpdateTransportGroupRequest>
{
    public UpdateTransportGroupRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(TransportGroup.NameMaxLength);
        RuleFor(x => x.Shift).MaximumLength(40);
    }
}
