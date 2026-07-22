namespace AttendanceManagement.Domain.Abstractions;

/// <summary>
/// Representa o desfecho de uma operacao: sucesso ou falha com um <see cref="Error"/>.
///
/// Por que isso em vez de lancar excecao? Regra de negocio violada nao e evento
/// excepcional: e um desfecho previsto. Excecao para controle de fluxo custa caro
/// (captura de stack trace) e esconde o caminho de erro da assinatura do metodo.
/// Com Result, o compilador te obriga a olhar o desfecho.
///
/// Substitui o INotificationService do TemplateCamadas com o mesmo objetivo.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        // Estados incoerentes (sucesso com erro, falha sem erro) sao bug de programacao,
        // nao entrada de usuario. Ai sim excecao e o instrumento certo.
        if (isSuccess && error != Error.None)
        {
            throw new ArgumentException("Um resultado de sucesso nao pode carregar erro.", nameof(error));
        }

        if (!isSuccess && error == Error.None)
        {
            throw new ArgumentException("Um resultado de falha precisa carregar um erro.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// <see cref="Result"/> que carrega um valor quando bem-sucedido.
/// </summary>
/// <typeparam name="TValue">Tipo do valor produzido em caso de sucesso.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error) => _value = value;

    /// <summary>
    /// Valor da operacao. Acessar em um resultado de falha e bug: lanca.
    /// Cheque <see cref="Result.IsSuccess"/> antes.
    /// </summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Nao ha valor em um resultado de falha.");

    /// <summary>Permite `return alumni;` onde se espera Result&lt;Alumni&gt;.</summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
