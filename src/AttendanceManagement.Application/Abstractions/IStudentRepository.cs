using AttendanceManagement.Application.Students;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Abstractions;

/// <summary>Acesso ao aluno do transportador. Escrita rastreia a entidade; leitura projeta o read model.</summary>
public interface IStudentRepository
{
    void Add(Student student);

    Task<Student?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default);

    Task<StudentResponse?> GetByIdAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentResponse>> ListAsync(Guid transporterId, CancellationToken cancellationToken = default);

    Task<bool> SchoolExistsAsync(Guid transporterId, Guid schoolId, CancellationToken cancellationToken = default);

    /// <summary>Matrícula ou vínculo de responsável ativo apontando para o aluno. Bloqueia a remoção.</summary>
    Task<bool> HasActiveLinksAsync(Guid studentId, CancellationToken cancellationToken = default);
}
