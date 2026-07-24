using AttendanceManagement.Application.Schools;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Abstractions;

/// <summary>Acesso à escola do transportador. Escrita rastreia a entidade; leitura projeta o read model.</summary>
public interface ISchoolRepository
{
    void Add(School school);

    Task<School?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default);

    Task<SchoolResponse?> GetByIdAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SchoolResponse>> ListAsync(Guid transporterId, CancellationToken cancellationToken = default);

    /// <summary>Nome já em uso por outra escola do tenant. <paramref name="excludeId"/> ignora a própria na edição.</summary>
    Task<bool> NameExistsAsync(Guid transporterId, string name, Guid? excludeId, CancellationToken cancellationToken = default);
}
