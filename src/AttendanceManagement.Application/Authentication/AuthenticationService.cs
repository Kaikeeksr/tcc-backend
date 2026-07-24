using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Application.Authentication;

/// <summary>Casos de uso de autenticação: entrar (login) e cadastrar o transportador.</summary>
public sealed class AuthenticationService(
    IUserAccountRepository userAccounts,
    ITransporterRepository transporters,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork)
{
    private const string TokenType = "Bearer";

    public async Task<Result<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result.Failure<LoginResponse>(
                Error.Validation("Auth.CredentialsRequired", "Provide email and password."));
        }

        var account = await userAccounts.GetByEmailAsync(request.Email, cancellationToken);

        // Mensagem genérica para os dois casos: dizer qual falhou entregaria a
        // lista de quem tem conta.
        if (account is null || !passwordHasher.Verify(request.Password, account.PasswordHash))
        {
            return Result.Failure<LoginResponse>(
                Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));
        }

        if (account.Status != UserStatus.Active)
        {
            var reason = account.Status == UserStatus.Blocked
                ? "This account is blocked."
                : "This account has not been activated yet.";

            return Result.Failure<LoginResponse>(Error.Unauthorized("Auth.AccountInactive", reason));
        }

        var identity = await userAccounts.GetIdentityAsync(account.Id, account.PrimaryRole, cancellationToken);

        if (identity is null)
        {
            return Result.Failure<LoginResponse>(
                Error.Failure("Auth.ProfileMissing", "Could not load this account's profile."));
        }

        return BuildResponse(account, identity);
    }

    /// <summary>Cria conta e perfil na mesma transação e já devolve o token (auto-login).</summary>
    public async Task<Result<LoginResponse>> RegisterTransporterAsync(
        RegisterTransporterRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Result.Failure<LoginResponse>(
                Error.Validation("Auth.PasswordRequired", "Password is required."));
        }

        // Pré-checagem amigável; o índice único do banco é a garantia real contra a corrida.
        if (await userAccounts.EmailExistsAsync(request.Email ?? string.Empty, cancellationToken))
        {
            return Result.Failure<LoginResponse>(
                Error.Conflict("Auth.EmailInUse", "An account with this email already exists."));
        }

        var accountResult = UserAccount.Create(
            request.Email,
            request.Phone,
            passwordHasher.Hash(request.Password),
            PrimaryRole.Transporter);

        if (accountResult.IsFailure)
        {
            return Result.Failure<LoginResponse>(accountResult.Error);
        }

        var account = accountResult.Value;

        var transporterResult = Transporter.Create(
            account.Id,
            request.Name,
            request.DocumentType,
            request.DocumentNumber);

        if (transporterResult.IsFailure)
        {
            return Result.Failure<LoginResponse>(transporterResult.Error);
        }

        var transporter = transporterResult.Value;

        // Sem confirmação de e-mail por ora: ativa na hora para o fluxo fechar
        // ponta a ponta.
        account.Activate();

        userAccounts.Add(account);
        transporters.Add(transporter);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var identity = new UserIdentity(transporter.Id, transporter.Name, transporter.Id);

        return BuildResponse(account, identity);
    }

    private Result<LoginResponse> BuildResponse(UserAccount account, UserIdentity identity)
    {
        var token = tokenGenerator.Generate(account, identity);

        var user = new AuthenticatedUser(
            account.Id,
            identity.DisplayName,
            account.Email,
            account.PrimaryRole,
            identity.ProfileId,
            identity.TransporterId);

        return Result.Success(new LoginResponse(token.Token, TokenType, token.ExpiresAtUtc, user));
    }
}
