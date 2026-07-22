namespace AttendanceManagement.Application.Abstractions;

/// <summary>
/// Violacao de indice unico detectada pelo banco no momento do commit.
///
/// Por que isso existe: a checagem previa de e-mail duplicado no
/// AlumniService tem uma janela de corrida. Duas requisicoes simultaneas com o
/// mesmo e-mail podem passar as duas pela checagem e so entao colidir no indice
/// unico. Sem este tratamento, a segunda viraria um 500 generico.
///
/// A Infrastructure traduz o erro nativo do PostgreSQL (SQLSTATE 23505) para
/// esta excecao, que e agnostica de banco; o GlobalExceptionHandler a converte
/// em 409 Conflict. Assim o detalhe do Npgsql nao vaza para fora da Infrastructure.
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
