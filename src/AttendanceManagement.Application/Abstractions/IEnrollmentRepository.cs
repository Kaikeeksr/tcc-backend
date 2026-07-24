using AttendanceManagement.Application.Enrollments;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Abstractions;

/// <summary>
/// Acesso à matrícula (vínculo aluno↔grupo). Como a matrícula não carrega o tenant,
/// o escopo é feito via o aluno/grupo relacionados, que carregam.
/// </summary>
public interface IEnrollmentRepository
{
    void Add(Enrollment enrollment);

    Task<Enrollment?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default);

    Task<bool> StudentExistsAsync(Guid transporterId, Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>Nome do grupo se ele pertence ao tenant; <c>null</c> caso contrário.</summary>
    Task<string?> GetGroupNameAsync(Guid transporterId, Guid groupId, CancellationToken cancellationToken = default);

    Task<bool> ActiveExistsAsync(Guid studentId, Guid groupId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EnrollmentResponse>> ListByStudentAsync(Guid studentId, CancellationToken cancellationToken = default);
}
