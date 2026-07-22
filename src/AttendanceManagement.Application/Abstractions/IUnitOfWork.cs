namespace AttendanceManagement.Application.Abstractions;

/// <summary>
/// Fronteira transacional. Os repositorios acumulam mudancas; o commit acontece
/// uma vez, aqui.
///
/// O TemplateCamadas chamava SaveChanges dentro de cada metodo do repositorio,
/// o que impede duas escritas relacionadas de compartilharem uma transacao.
/// Separar quem-altera de quem-confirma resolve isso.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Confirma as mudancas pendentes.
    /// </summary>
    /// <exception cref="UniqueConstraintViolationException">
    /// Quando a gravacao viola um indice unico do banco.
    /// </exception>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
