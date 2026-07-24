using AttendanceManagement.Application.Vehicles;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Abstractions;

/// <summary>Acesso ao veículo do transportador. Escrita rastreia a entidade; leitura projeta o read model.</summary>
public interface IVehicleRepository
{
    void Add(Vehicle vehicle);

    /// <summary>Entidade rastreada para edição/remoção, já escopada ao tenant.</summary>
    Task<Vehicle?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default);

    Task<VehicleResponse?> GetByIdAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VehicleResponse>> ListAsync(Guid transporterId, CancellationToken cancellationToken = default);

    /// <summary>Placa já em uso por outro veículo do tenant. <paramref name="excludeId"/> ignora o próprio na edição.</summary>
    Task<bool> PlateExistsAsync(Guid transporterId, string plate, Guid? excludeId, CancellationToken cancellationToken = default);
}
