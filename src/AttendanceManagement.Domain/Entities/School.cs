using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.ValueObjects;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// Escola, cadastrada por transporter. Liga-se ao STUDENT, não ao grupo — por
/// isso um transport_group pode ter alunos de escolas diferentes.
/// </summary>
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

    public static Result<School> Create(Guid transporterId, string? name, Address? address)
    {
        if (transporterId == Guid.Empty)
        {
            return Result.Failure<School>(Error.Validation("School.TransporterRequired", "O transportador é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<School>(Error.Validation("School.NameRequired", "O nome é obrigatório."));
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
}
