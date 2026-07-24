using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.ValueObjects;

namespace AttendanceManagement.Domain.Entities;

/// <summary>Escola, cadastrada por transporter. Liga-se ao student, não ao grupo.</summary>
public sealed class School : SoftDeletableEntity
{
    public const int NameMaxLength = 150;

    private School(Guid id, Guid transporterId, string name, Address address)
        : base(id)
    {
        TransporterId = transporterId;
        Name = name;
        Address = address;
    }

    private School()
    {
    }

    public Guid TransporterId { get; private set; }

    public string Name { get; private set; } = null!;

    public Address Address { get; private set; } = null!;

    public Transporter Transporter { get; private set; } = null!;

    public static Result<School> Create(Guid transporterId, string? name, Address? address)
    {
        if (transporterId == Guid.Empty)
        {
            return Result.Failure<School>(Error.Validation("School.TransporterRequired", "Transporter is required."));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<School>(Error.Validation("School.NameRequired", "Name is required."));
        }

        return Result.Success(new School(
            Guid.CreateVersion7(),
            transporterId,
            name.Trim(),
            address ?? Address.Empty()));
    }

    public void SetAddress(Address address)
    {
        Address = address;
        Touch();
    }

    public Result Update(string? name, Address? address)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Error.Validation("School.NameRequired", "Name is required."));
        }

        Name = name.Trim();
        Address = address ?? Address.Empty();
        Touch();
        return Result.Success();
    }
}
