using AttendanceManagement.Application.Guardians;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Abstractions;

/// <summary>Acesso ao responsável do transportador. A escrita rastreia o responsável com a conta de login.</summary>
public interface IGuardianRepository
{
    void Add(Guardian guardian);

    /// <summary>Responsável rastreado com a <c>UserAccount</c> carregada, para editar/remover e poder bloquear o login.</summary>
    Task<Guardian?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default);

    Task<GuardianResponse?> GetByIdAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GuardianResponse>> ListAsync(Guid transporterId, CancellationToken cancellationToken = default);

    /// <summary>O responsável tem algum aluno vinculado ativo? Bloqueia a remoção.</summary>
    Task<bool> HasActiveStudentsAsync(Guid guardianId, CancellationToken cancellationToken = default);
}
