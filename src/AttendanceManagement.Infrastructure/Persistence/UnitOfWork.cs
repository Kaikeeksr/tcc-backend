using AttendanceManagement.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AttendanceManagement.Infrastructure.Persistence;

/// <summary>
/// Confirma no banco tudo o que os repositorios acumularam na requisicao.
/// </summary>
internal sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            // Traduz o erro nativo do PostgreSQL para uma excecao agnostica de banco.
            // E esta fronteira que impede o Npgsql de vazar para a Application:
            // de fora daqui, ninguem precisa saber o que e um SQLSTATE.
            throw new UniqueConstraintViolationException(
                "A gravação violou uma restrição de unicidade.",
                exception);
        }
    }

    /// <summary>
    /// SQLSTATE 23505 = unique_violation no PostgreSQL.
    /// </summary>
    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
        };
}
