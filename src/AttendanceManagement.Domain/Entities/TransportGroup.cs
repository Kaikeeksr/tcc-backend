using AttendanceManagement.Domain.Abstractions;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// Grupo operacional de alunos (a "turma" da van), com o veículo e o assistant
/// designados. Não confundir com <c>Student.Grade</c> (a série escolar).
/// </summary>
public sealed class TransportGroup : SoftDeletableEntity
{
    public const int NameMaxLength = 120;

    private TransportGroup(Guid id, Guid transporterId, string name, string? shift)
        : base(id)
    {
        TransporterId = transporterId;
        Name = name;
        Shift = shift;
    }

    private TransportGroup()
    {
    }

    public Guid TransporterId { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Shift { get; private set; }

    public Guid? VehicleId { get; private set; }

    public Guid? AssistantId { get; private set; }

    public Transporter Transporter { get; private set; } = null!;

    public Vehicle? Vehicle { get; private set; }

    public Assistant? Assistant { get; private set; }

    public static Result<TransportGroup> Create(Guid transporterId, string? name, string? shift)
    {
        if (transporterId == Guid.Empty)
        {
            return Result.Failure<TransportGroup>(Error.Validation("TransportGroup.TransporterRequired", "Transporter is required."));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<TransportGroup>(Error.Validation("TransportGroup.NameRequired", "Name is required."));
        }

        return Result.Success(new TransportGroup(
            Guid.CreateVersion7(),
            transporterId,
            name.Trim(),
            string.IsNullOrWhiteSpace(shift) ? null : shift.Trim()));
    }

    public void AssignCrew(Guid? vehicleId, Guid? assistantId)
    {
        VehicleId = vehicleId == Guid.Empty ? null : vehicleId;
        AssistantId = assistantId == Guid.Empty ? null : assistantId;
        Touch();
    }

    public Result Update(string? name, string? shift)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Error.Validation("TransportGroup.NameRequired", "Name is required."));
        }

        Name = name.Trim();
        Shift = string.IsNullOrWhiteSpace(shift) ? null : shift.Trim();
        Touch();
        return Result.Success();
    }
}
