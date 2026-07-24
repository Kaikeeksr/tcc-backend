using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.Common;
using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Domain.Enums;
using AttendanceManagement.Domain.ValueObjects;

namespace AttendanceManagement.Application.Guardians;

/// <summary>
/// Casos de uso do responsável. Cadastrar cria também a conta de login (o
/// responsável sempre acessa o app), na mesma transação.
/// </summary>
public sealed class GuardianService(
    IGuardianRepository repository,
    IUserAccountRepository userAccounts,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
{
    public Task<IReadOnlyList<GuardianResponse>> ListAsync(
        Guid transporterId,
        CancellationToken cancellationToken = default) =>
        repository.ListAsync(transporterId, cancellationToken);

    public async Task<Result<GuardianResponse>> GetByIdAsync(
        Guid transporterId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var guardian = await repository.GetByIdAsync(transporterId, id, cancellationToken);

        return guardian is null
            ? Result.Failure<GuardianResponse>(Error.NotFound("Guardian.NotFound", "Guardian not found."))
            : Result.Success(guardian);
    }

    public async Task<Result<GuardianResponse>> CreateAsync(
        Guid transporterId,
        CreateGuardianRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Result.Failure<GuardianResponse>(
                Error.Validation("Guardian.PasswordRequired", "Password is required."));
        }

        if (await userAccounts.EmailExistsAsync(request.Email ?? string.Empty, cancellationToken))
        {
            return Result.Failure<GuardianResponse>(
                Error.Conflict("Guardian.EmailInUse", "An account with this email already exists."));
        }

        var accountResult = UserAccount.Create(
            request.Email,
            request.Contact?.Mobile,
            passwordHasher.Hash(request.Password),
            PrimaryRole.Guardian);

        if (accountResult.IsFailure)
        {
            return Result.Failure<GuardianResponse>(accountResult.Error);
        }

        var account = accountResult.Value;

        var guardianResult = Guardian.Create(transporterId, account.Id, request.Name, request.Address?.ToValueObject());
        if (guardianResult.IsFailure)
        {
            return Result.Failure<GuardianResponse>(guardianResult.Error);
        }

        var guardian = guardianResult.Value;
        ApplyContact(guardian, request.Contact);
        account.Activate();

        userAccounts.Add(account);
        repository.Add(guardian);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(guardian, account.Email));
    }

    public async Task<Result<GuardianResponse>> UpdateAsync(
        Guid transporterId,
        Guid id,
        UpdateGuardianRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var guardian = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (guardian is null)
        {
            return Result.Failure<GuardianResponse>(Error.NotFound("Guardian.NotFound", "Guardian not found."));
        }

        var result = guardian.Rename(request.Name);
        if (result.IsFailure)
        {
            return Result.Failure<GuardianResponse>(result.Error);
        }

        ApplyContact(guardian, request.Contact);
        guardian.SetAddress(request.Address?.ToValueObject() ?? Address.Empty());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(guardian, guardian.UserAccount.Email));
    }

    public async Task<Result> DeleteAsync(
        Guid transporterId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var guardian = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (guardian is null)
        {
            return Result.Failure(Error.NotFound("Guardian.NotFound", "Guardian not found."));
        }

        if (await repository.HasActiveStudentsAsync(id, cancellationToken))
        {
            return Result.Failure(Error.Conflict(
                "Guardian.HasStudents",
                "The guardian still has a linked student. Unlink them first."));
        }

        guardian.SoftDelete();
        guardian.UserAccount.Block();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static void ApplyContact(Guardian guardian, GuardianContactPayload? contact) =>
        guardian.SetContact(contact?.Phone, contact?.Mobile, contact?.Whatsapp, contact?.ContactEmail);

    private static GuardianResponse ToResponse(Guardian guardian, string email) =>
        new(
            guardian.Id,
            guardian.Name,
            email,
            guardian.UserAccountId,
            new GuardianContactPayload(guardian.Phone, guardian.Mobile, guardian.Whatsapp, guardian.ContactEmail),
            AddressPayload.FromValueObject(guardian.Address));
}
