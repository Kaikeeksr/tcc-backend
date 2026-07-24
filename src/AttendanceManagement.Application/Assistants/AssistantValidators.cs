using AttendanceManagement.Domain.Entities;
using FluentValidation;

namespace AttendanceManagement.Application.Assistants;

public sealed class CreateAssistantRequestValidator : AbstractValidator<CreateAssistantRequest>
{
    public CreateAssistantRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(UserAccount.EmailMaxLength);
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public sealed class UpdateAssistantRequestValidator : AbstractValidator<UpdateAssistantRequest>
{
    public UpdateAssistantRequestValidator() =>
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
}
