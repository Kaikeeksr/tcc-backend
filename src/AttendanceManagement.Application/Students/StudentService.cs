using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Application.Students;

/// <summary>Casos de uso do aluno, sempre escopados ao tenant do chamador.</summary>
public sealed class StudentService(
    IStudentRepository repository,
    IUserAccountRepository userAccounts,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
{
    public Task<IReadOnlyList<StudentResponse>> ListAsync(
        Guid transporterId,
        CancellationToken cancellationToken = default) =>
        repository.ListAsync(transporterId, cancellationToken);

    public async Task<Result<StudentResponse>> GetByIdAsync(
        Guid transporterId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var student = await repository.GetByIdAsync(transporterId, id, cancellationToken);

        return student is null
            ? Result.Failure<StudentResponse>(Error.NotFound("Student.NotFound", "Student not found."))
            : Result.Success(student);
    }

    public async Task<Result<StudentResponse>> CreateAsync(
        Guid transporterId,
        CreateStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var schoolCheck = await EnsureSchoolAsync(transporterId, request.SchoolId, cancellationToken);
        if (schoolCheck.IsFailure)
        {
            return Result.Failure<StudentResponse>(schoolCheck.Error);
        }

        var result = Student.Create(transporterId, request.Name, request.BirthDate, request.Grade, request.SchoolId);
        if (result.IsFailure)
        {
            return Result.Failure<StudentResponse>(result.Error);
        }

        var student = result.Value;
        repository.Add(student);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(student));
    }

    public async Task<Result<StudentResponse>> UpdateAsync(
        Guid transporterId,
        Guid id,
        UpdateStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var student = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (student is null)
        {
            return Result.Failure<StudentResponse>(Error.NotFound("Student.NotFound", "Student not found."));
        }

        var schoolCheck = await EnsureSchoolAsync(transporterId, request.SchoolId, cancellationToken);
        if (schoolCheck.IsFailure)
        {
            return Result.Failure<StudentResponse>(schoolCheck.Error);
        }

        var result = student.Update(request.Name, request.BirthDate, request.Grade);
        if (result.IsFailure)
        {
            return Result.Failure<StudentResponse>(result.Error);
        }

        student.TransferToSchool(request.SchoolId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(student));
    }

    public async Task<Result> DeleteAsync(
        Guid transporterId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var student = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (student is null)
        {
            return Result.Failure(Error.NotFound("Student.NotFound", "Student not found."));
        }

        if (await repository.HasActiveLinksAsync(id, cancellationToken))
        {
            return Result.Failure(Error.Conflict(
                "Student.HasActiveLinks",
                "The student still has an active enrollment or guardian link. Remove them first."));
        }

        student.SoftDelete();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <summary>Cria o login opcional do aluno, para ele acompanhar a própria frequência no app.</summary>
    public async Task<Result<StudentResponse>> CreateLoginAsync(
        Guid transporterId,
        Guid id,
        CreateStudentLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var student = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (student is null)
        {
            return Result.Failure<StudentResponse>(Error.NotFound("Student.NotFound", "Student not found."));
        }

        if (student.UserAccountId is not null)
        {
            return Result.Failure<StudentResponse>(Error.Conflict("Student.AlreadyHasLogin", "This student already has a login."));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Result.Failure<StudentResponse>(Error.Validation("Student.PasswordRequired", "Password is required."));
        }

        if (await userAccounts.EmailExistsAsync(request.Email ?? string.Empty, cancellationToken))
        {
            return Result.Failure<StudentResponse>(Error.Conflict("Student.EmailInUse", "An account with this email already exists."));
        }

        var accountResult = UserAccount.Create(request.Email, null, passwordHasher.Hash(request.Password), PrimaryRole.Student);
        if (accountResult.IsFailure)
        {
            return Result.Failure<StudentResponse>(accountResult.Error);
        }

        var account = accountResult.Value;
        account.Activate();

        userAccounts.Add(account);
        student.SetLogin(account.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(student));
    }

    private async Task<Result> EnsureSchoolAsync(Guid transporterId, Guid? schoolId, CancellationToken cancellationToken)
    {
        if (schoolId is not { } id || id == Guid.Empty)
        {
            return Result.Success();
        }

        return await repository.SchoolExistsAsync(transporterId, id, cancellationToken)
            ? Result.Success()
            : Result.Failure(Error.NotFound("Student.SchoolNotFound", "School not found."));
    }

    private static StudentResponse ToResponse(Student student) =>
        new(student.Id, student.Name, student.BirthDate, student.Grade, student.SchoolId, student.UserAccountId is not null);
}
