using AttendanceManagement.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AttendanceManagement.Infrastructure.Persistence;

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
            // Traduz o erro nativo do Postgres numa exceção agnóstica de banco.
            throw new UniqueConstraintViolationException(
                "A gravação violou uma restrição de unicidade.",
                exception);
        }
    }

    // SQLSTATE 23505 = unique_violation.
    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
        };
}
