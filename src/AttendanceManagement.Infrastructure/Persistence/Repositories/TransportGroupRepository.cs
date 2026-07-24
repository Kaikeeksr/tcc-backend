using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.TransportGroups;
using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Persistence.Repositories;

internal sealed class TransportGroupRepository(AppDbContext context) : ITransportGroupRepository
{
    public void Add(TransportGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);

        context.TransportGroups.Add(group);
    }

    public Task<TransportGroup?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        context.TransportGroups
            .AsTracking()
            .FirstOrDefaultAsync(g => g.Id == id && g.TransporterId == transporterId, cancellationToken);

    public Task<TransportGroupResponse?> GetByIdAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        context.TransportGroups
            .Where(g => g.Id == id && g.TransporterId == transporterId)
            .Select(g => new TransportGroupResponse(g.Id, g.Name, g.Shift, g.VehicleId, g.AssistantId))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<TransportGroupResponse>> ListAsync(Guid transporterId, CancellationToken cancellationToken = default) =>
        await context.TransportGroups
            .Where(g => g.TransporterId == transporterId)
            .OrderBy(g => g.Name)
            .Select(g => new TransportGroupResponse(g.Id, g.Name, g.Shift, g.VehicleId, g.AssistantId))
            .ToListAsync(cancellationToken);

    public Task<bool> VehicleExistsAsync(Guid transporterId, Guid vehicleId, CancellationToken cancellationToken = default) =>
        context.Vehicles.AnyAsync(v => v.Id == vehicleId && v.TransporterId == transporterId, cancellationToken);

    public Task<bool> AssistantExistsAsync(Guid transporterId, Guid assistantId, CancellationToken cancellationToken = default) =>
        context.Assistants.AnyAsync(a => a.Id == assistantId && a.TransporterId == transporterId, cancellationToken);

    public Task<bool> HasActiveEnrollmentsAsync(Guid groupId, CancellationToken cancellationToken = default) =>
        context.Enrollments.AnyAsync(e => e.TransportGroupId == groupId && e.Active, cancellationToken);
}
