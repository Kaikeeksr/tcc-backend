using FluentValidation;

namespace AttendanceManagement.Application.Vehicles;

public sealed class CreateVehicleRequestValidator : AbstractValidator<CreateVehicleRequest>
{
    public CreateVehicleRequestValidator()
    {
        RuleFor(x => x.Plate).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Model).MaximumLength(80);
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(0).When(x => x.Capacity.HasValue);
    }
}

public sealed class UpdateVehicleRequestValidator : AbstractValidator<UpdateVehicleRequest>
{
    public UpdateVehicleRequestValidator()
    {
        RuleFor(x => x.Plate).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Model).MaximumLength(80);
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(0).When(x => x.Capacity.HasValue);
    }
}
