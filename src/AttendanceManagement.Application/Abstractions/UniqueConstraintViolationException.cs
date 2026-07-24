namespace AttendanceManagement.Application.Abstractions;

/// <summary>
/// Violação de índice único detectada pelo banco no commit. A Infrastructure
/// traduz o erro nativo do PostgreSQL (SQLSTATE 23505) para esta exceção
/// agnóstica; o GlobalExceptionHandler a converte em 409.
/// </summary>
public sealed class UniqueConstraintViolationException : Exception
{
    public UniqueConstraintViolationException()
        : base("Violação de restrição de unicidade.")
    {
    }

    public UniqueConstraintViolationException(string message)
        : base(message)
    {
    }

    public UniqueConstraintViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
