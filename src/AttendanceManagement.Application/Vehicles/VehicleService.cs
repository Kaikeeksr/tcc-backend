using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Vehicles;

/// <summary>Casos de uso do veículo, sempre escopados ao tenant do chamador.</summary>
public sealed class VehicleService(IVehicleRepository repository, IUnitOfWork unitOfWork)
{
    public Task<IReadOnlyList<VehicleResponse>> ListAsync(
        Guid transporterId,
        CancellationToken cancellationToken = default) =>
        repository.ListAsync(transporterId, cancellationToken);

    public async Task<Result<VehicleResponse>> GetByIdAsync(
        Guid transporterId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await repository.GetByIdAsync(transporterId, id, cancellationToken);

        return vehicle is null
            ? Result.Failure<VehicleResponse>(Error.NotFound("Vehicle.NotFound", "Vehicle not found."))
            : Result.Success(vehicle);
    }

    public async Task<Result<VehicleResponse>> CreateAsync(
        Guid transporterId,
        CreateVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = Vehicle.Create(transporterId, request.Plate, request.Model, request.Capacity);
        if (result.IsFailure)
        {
            return Result.Failure<VehicleResponse>(result.Error);
        }

        var vehicle = result.Value;

        if (await repository.PlateExistsAsync(transporterId, vehicle.Plate, null, cancellationToken))
        {
            return Result.Failure<VehicleResponse>(
                Error.Conflict("Vehicle.PlateInUse", "A vehicle with this plate already exists."));
        }

        repository.Add(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(vehicle));
    }

    public async Task<Result<VehicleResponse>> UpdateAsync(
        Guid transporterId,
        Guid id,
        UpdateVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var vehicle = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (vehicle is null)
        {
            return Result.Failure<VehicleResponse>(Error.NotFound("Vehicle.NotFound", "Vehicle not found."));
        }

        var result = vehicle.Update(request.Plate, request.Model, request.Capacity);
        if (result.IsFailure)
        {
            return Result.Failure<VehicleResponse>(result.Error);
        }

        if (await repository.PlateExistsAsync(transporterId, vehicle.Plate, id, cancellationToken))
        {
            return Result.Failure<VehicleResponse>(
                Error.Conflict("Vehicle.PlateInUse", "A vehicle with this plate already exists."));
        }

        vehicle.SetActive(request.Active);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(vehicle));
    }

    public async Task<Result> DeleteAsync(
        Guid transporterId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (vehicle is null)
        {
            return Result.Failure(Error.NotFound("Vehicle.NotFound", "Vehicle not found."));
        }

        vehicle.SoftDelete();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static VehicleResponse ToResponse(Vehicle vehicle) =>
        new(vehicle.Id, vehicle.Plate, vehicle.Model, vehicle.Capacity, vehicle.Active);
}
