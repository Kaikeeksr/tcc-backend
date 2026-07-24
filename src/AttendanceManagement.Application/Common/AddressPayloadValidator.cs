using FluentValidation;

namespace AttendanceManagement.Application.Common;

/// <summary>Tamanhos espelham as colunas do owned type Address; coordenadas dentro do intervalo válido.</summary>
public sealed class AddressPayloadValidator : AbstractValidator<AddressPayload>
{
    public AddressPayloadValidator()
    {
        RuleFor(x => x.Street).MaximumLength(200);
        RuleFor(x => x.Number).MaximumLength(20);
        RuleFor(x => x.Complement).MaximumLength(100);
        RuleFor(x => x.District).MaximumLength(100);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.State).MaximumLength(2);
        RuleFor(x => x.PostalCode).MaximumLength(12);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
    }
}
