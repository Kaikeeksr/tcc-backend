using AttendanceManagement.Application.Transporters;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Abstractions;

/// <summary>
/// Acesso ao transportador (raiz do tenant). Escrita devolve entidade; leitura
/// devolve read model projetado.
/// </summary>
public interface ITransporterRepository
{
    Task<Transporter?> GetByUserAccountAsync(
        Guid userAccountId,
        CancellationToken cancellationToken = default);

    /// <summary>Busca o motorista pelo e-mail da conta. Para o fluxo de autenticação, antes de haver um id.</summary>
    Task<Transporter?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>Monta a equipe do motorista: ele e cada grupo com monitor, veículo e matrículas ativas.</summary>
    Task<TransportTeam?> GetTeamByUserAccountAsync(
        Guid userAccountId,
        CancellationToken cancellationToken = default);

    /// <summary>Registra um novo transportador. O commit fica por conta do <see cref="IUnitOfWork"/>.</summary>
    void Add(Transporter transporter);
}
