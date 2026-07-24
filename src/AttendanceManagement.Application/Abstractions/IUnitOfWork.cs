namespace AttendanceManagement.Application.Abstractions;

/// <summary>
/// Fronteira transacional: os repositórios acumulam mudanças, o commit acontece
/// uma vez, aqui.
/// </summary>
public interface IUnitOfWork
{
    /// <exception cref="UniqueConstraintViolationException">Quando a gravação viola um índice único.</exception>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
