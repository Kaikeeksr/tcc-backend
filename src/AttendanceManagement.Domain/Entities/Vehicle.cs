using AttendanceManagement.Domain.Abstractions;

namespace AttendanceManagement.Domain.Entities;

public sealed class Vehicle : SoftDeletableEntity
{
    private Vehicle(Guid id, Guid transporterId, string plate, string? model, int? capacity)
        : base(id)
    {
        TransporterId = transporterId;
        Plate = plate;
        Model = model;
        Capacity = capacity;
        Active = true;
    }

    private Vehicle()
    {
    }

    public Guid TransporterId { get; private set; }

    /// <summary>Placa normalizada: sem hífen, maiúscula.</summary>
    public string Plate { get; private set; } = null!;

    public string? Model { get; private set; }

    public int? Capacity { get; private set; }

    public bool Active { get; private set; }

    public static Result<Vehicle> Create(Guid transporterId, string? plate, string? model, int? capacity)
    {
        if (transporterId == Guid.Empty)
        {
            return Result.Failure<Vehicle>(Error.Validation("Vehicle.TransporterRequired", "O transportador é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(plate))
        {
            return Result.Failure<Vehicle>(Error.Validation("Vehicle.PlateRequired", "A placa é obrigatória."));
        }

        if (capacity is < 0)
        {
            return Result.Failure<Vehicle>(Error.Validation("Vehicle.CapacityInvalid", "A capacidade não pode ser negativa."));
        }

        var normalizedPlate = plate.Trim().Replace("-", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

        return Result.Success(new Vehicle(
            Guid.CreateVersion7(),
            transporterId,
            normalizedPlate,
            string.IsNullOrWhiteSpace(model) ? null : model.Trim(),
            capacity));
    }

    public void SetActive(bool active)
    {
        Active = active;
        Touch();
    }
}
