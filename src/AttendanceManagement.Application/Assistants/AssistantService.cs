using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Application.Assistants;

/// <summary>
/// Casos de uso do monitor. Cadastrar cria também a conta de login (o monitor
/// sempre acessa o app), na mesma transação.
/// </summary>
public sealed class AssistantService(
    IAssistantRepository repository,
    IUserAccountRepository userAccounts,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
{
    public Task<IReadOnlyList<AssistantResponse>> ListAsync(
        Guid transporterId,
        CancellationToken cancellationToken = default) =>
        repository.ListAsync(transporterId, cancellationToken);

    public async Task<Result<AssistantResponse>> GetByIdAsync(
        Guid transporterId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var assistant = await repository.GetByIdAsync(transporterId, id, cancellationToken);

        return assistant is null
            ? Result.Failure<AssistantResponse>(Error.NotFound("Assistant.NotFound", "Assistant not found."))
            : Result.Success(assistant);
    }

    public async Task<Result<AssistantResponse>> CreateAsync(
        Guid transporterId,
        CreateAssistantRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Result.Failure<AssistantResponse>(
                Error.Validation("Assistant.PasswordRequired", "Password is required."));
        }

        if (await userAccounts.EmailExistsAsync(request.Email ?? string.Empty, cancellationToken))
        {
            return Result.Failure<AssistantResponse>(
                Error.Conflict("Assistant.EmailInUse", "An account with this email already exists."));
        }

        var accountResult = UserAccount.Create(
            request.Email,
            request.Phone,
            passwordHasher.Hash(request.Password),
            PrimaryRole.Assistant);

        if (accountResult.IsFailure)
        {
            return Result.Failure<AssistantResponse>(accountResult.Error);
        }

        var account = accountResult.Value;

        var assistantResult = Assistant.Create(transporterId, account.Id, request.Name);
        if (assistantResult.IsFailure)
        {
            return Result.Failure<AssistantResponse>(assistantResult.Error);
        }

        var assistant = assistantResult.Value;
        account.Activate();

        userAccounts.Add(account);
        repository.Add(assistant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(assistant, account.Email));
    }

    public async Task<Result<AssistantResponse>> UpdateAsync(
        Guid transporterId,
        Guid id,
        UpdateAssistantRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var assistant = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (assistant is null)
        {
            return Result.Failure<AssistantResponse>(Error.NotFound("Assistant.NotFound", "Assistant not found."));
        }

        var result = assistant.Rename(request.Name);
        if (result.IsFailure)
        {
            return Result.Failure<AssistantResponse>(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(assistant, assistant.UserAccount.Email));
    }

    public async Task<Result> DeleteAsync(
        Guid transporterId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var assistant = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (assistant is null)
        {
            return Result.Failure(Error.NotFound("Assistant.NotFound", "Assistant not found."));
        }

        if (await repository.IsAssignedToGroupAsync(id, cancellationToken))
        {
            return Result.Failure(Error.Conflict(
                "Assistant.Assigned",
                "The assistant is still assigned to a group. Reassign the group's crew first."));
        }

        assistant.SoftDelete();
        assistant.UserAccount.Block();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static AssistantResponse ToResponse(Assistant assistant, string email) =>
        new(assistant.Id, assistant.Name, email, assistant.UserAccountId);
}
