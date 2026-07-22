namespace AttendanceManagement.Domain.Abstractions;

/// <summary>
/// Um erro de dominio: codigo estavel para o cliente, descricao legivel e tipo.
/// E um record, entao a comparacao por valor sai de graca.
/// </summary>
/// <param name="Code">Identificador estavel, ex.: "Alumni.EmailAlreadyInUse".</param>
/// <param name="Description">Mensagem legivel por humanos.</param>
/// <param name="Type">Natureza do erro, usada para mapear o status HTTP.</param>
public sealed record Error(string Code, string Description, ErrorType Type)
{
    /// <summary>Ausencia de erro. Usado internamente por <see cref="Result"/>.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);
}
