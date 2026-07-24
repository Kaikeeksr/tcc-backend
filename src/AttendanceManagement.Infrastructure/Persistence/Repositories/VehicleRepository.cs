using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.Vehicles;
using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Persistence.Repositories;

internal sealed class VehicleRepository(AppDbContext context) : IVehicleRepository
{
    public void Add(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        context.Vehicles.Add(vehicle);
    }

    public Task<Vehicle?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        context.Vehicles
            .AsTracking()
            .FirstOrDefaultAsync(v => v.Id == id && v.TransporterId == transporterId, cancellationToken);

    public Task<VehicleResponse?> GetByIdAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        context.Vehicles
            .Where(v => v.Id == id && v.TransporterId == transporterId)
            .Select(v => new VehicleResponse(v.Id, v.Plate, v.Model, v.Capacity, v.Active))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<VehicleResponse>> ListAsync(Guid transporterId, CancellationToken cancellationToken = default) =>
        await context.Vehicles
            .Where(v => v.TransporterId == transporterId)
            .OrderBy(v => v.Plate)
            .Select(v => new VehicleResponse(v.Id, v.Plate, v.Model, v.Capacity, v.Active))
            .ToListAsync(cancellationToken);

    public Task<bool> PlateExistsAsync(Guid transporterId, string plate, Guid? excludeId, CancellationToken cancellationToken = default) =>
        context.Vehicles
            .Where(v => v.TransporterId == transporterId && v.Plate == plate)
            .Where(v => excludeId == null || v.Id != excludeId)
            .AnyAsync(cancellationToken);
}
