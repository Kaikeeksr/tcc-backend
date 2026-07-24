namespace AttendanceManagement.Domain.ValueObjects;

/// <summary>Value object embutido (owned type): vira colunas <c>address_*</c> na tabela dona.</summary>
public sealed class Address
{
    public Address(
        string? street,
        string? number,
        string? complement,
        string? district,
        string? city,
        string? state,
        string? postalCode,
        double? latitude,
        double? longitude)
    {
        Street = street?.Trim();
        Number = number?.Trim();
        Complement = complement?.Trim();
        District = district?.Trim();
        City = city?.Trim();
        State = state?.Trim().ToUpperInvariant();
        PostalCode = postalCode?.Trim();
        Latitude = latitude;
        Longitude = longitude;
    }

    private Address()
    {
    }

    public string? Street { get; private set; }

    public string? Number { get; private set; }

    public string? Complement { get; private set; }

    public string? District { get; private set; }

    public string? City { get; private set; }

    public string? State { get; private set; }

    public string? PostalCode { get; private set; }

    public double? Latitude { get; private set; }

    public double? Longitude { get; private set; }

    public static Address Empty() => new(null, null, null, null, null, null, null, null, null);
}
