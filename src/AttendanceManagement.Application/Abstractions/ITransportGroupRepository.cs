using AttendanceManagement.Application.TransportGroups;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Abstractions;

/// <summary>Acesso ao grupo de transporte e às validações de posse da equipe designada.</summary>
public interface ITransportGroupRepository
{
    void Add(TransportGroup group);

    Task<TransportGroup?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default);

    Task<TransportGroupResponse?> GetByIdAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TransportGroupResponse>> ListAsync(Guid transporterId, CancellationToken cancellationToken = default);

    Task<bool> VehicleExistsAsync(Guid transporterId, Guid vehicleId, CancellationToken cancellationToken = default);

    Task<bool> AssistantExistsAsync(Guid transporterId, Guid assistantId, CancellationToken cancellationToken = default);

    /// <summary>Existe matrícula ativa apontando para o grupo? Bloqueia a remoção.</summary>
    Task<bool> HasActiveEnrollmentsAsync(Guid groupId, CancellationToken cancellationToken = default);
}
