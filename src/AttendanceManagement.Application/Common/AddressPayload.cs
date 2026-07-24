using System.Text.Json.Serialization;
using AttendanceManagement.Domain.ValueObjects;

namespace AttendanceManagement.Application.Common;

/// <summary>Endereço no payload JSON. Espelha o value object <see cref="Address"/>.</summary>
public sealed record AddressPayload(
    [property: JsonPropertyName("street")] string? Street,
    [property: JsonPropertyName("number")] string? Number,
    [property: JsonPropertyName("complement")] string? Complement,
    [property: JsonPropertyName("district")] string? District,
    [property: JsonPropertyName("city")] string? City,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("postal_code")] string? PostalCode,
    [property: JsonPropertyName("latitude")] double? Latitude,
    [property: JsonPropertyName("longitude")] double? Longitude)
{
    public Address ToValueObject() =>
        new(Street, Number, Complement, District, City, State, PostalCode, Latitude, Longitude);

    public static AddressPayload FromValueObject(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);

        return new AddressPayload(
            address.Street,
            address.Number,
            address.Complement,
            address.District,
            address.City,
            address.State,
            address.PostalCode,
            address.Latitude,
            address.Longitude);
    }
}
