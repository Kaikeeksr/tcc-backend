using AttendanceManagement.Application.GuardianStudents;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Abstractions;

/// <summary>
/// Acesso ao vínculo responsável↔aluno. O escopo por tenant é feito via o aluno
/// relacionado, que carrega o transporter.
/// </summary>
public interface IGuardianStudentRepository
{
    void Add(GuardianStudent link);

    Task<GuardianStudent?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>Contato principal ativo do aluno (opcionalmente ignorando um vínculo), rastreado para rebaixar.</summary>
    Task<GuardianStudent?> GetActivePrimaryAsync(Guid studentId, Guid? excludeId, CancellationToken cancellationToken = default);

    Task<bool> StudentExistsAsync(Guid transporterId, Guid studentId, CancellationToken cancellationToken = default);

    Task<bool> GuardianExistsAsync(Guid transporterId, Guid guardianId, CancellationToken cancellationToken = default);

    Task<bool> ActivePairExistsAsync(Guid guardianId, Guid studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GuardianStudentResponse>> ListByStudentAsync(Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>Filhos ativos vinculados a um responsável (pela tela "meus filhos").</summary>
    Task<IReadOnlyList<MyChildResponse>> ListByGuardianAsync(Guid guardianId, CancellationToken cancellationToken = default);
}
