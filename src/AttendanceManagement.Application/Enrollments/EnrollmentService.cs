using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Enrollments;

/// <summary>Casos de uso da matrícula: matricular um aluno num grupo, encerrar e listar.</summary>
public sealed class EnrollmentService(IEnrollmentRepository repository, IUnitOfWork unitOfWork)
{
    public async Task<Result<IReadOnlyList<EnrollmentResponse>>> ListByStudentAsync(
        Guid transporterId,
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        if (!await repository.StudentExistsAsync(transporterId, studentId, cancellationToken))
        {
            return Result.Failure<IReadOnlyList<EnrollmentResponse>>(
                Error.NotFound("Student.NotFound", "Student not found."));
        }

        return Result.Success(await repository.ListByStudentAsync(studentId, cancellationToken));
    }

    public async Task<Result<EnrollmentResponse>> EnrollAsync(
        Guid transporterId,
        Guid studentId,
        EnrollStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await repository.StudentExistsAsync(transporterId, studentId, cancellationToken))
        {
            return Result.Failure<EnrollmentResponse>(Error.NotFound("Student.NotFound", "Student not found."));
        }

        var groupName = await repository.GetGroupNameAsync(transporterId, request.TransportGroupId, cancellationToken);
        if (groupName is null)
        {
            return Result.Failure<EnrollmentResponse>(
                Error.NotFound("Enrollment.GroupNotFound", "Transport group not found."));
        }

        if (await repository.ActiveExistsAsync(studentId, request.TransportGroupId, cancellationToken))
        {
            return Result.Failure<EnrollmentResponse>(
                Error.Conflict("Enrollment.AlreadyEnrolled", "The student is already enrolled in this group."));
        }

        var result = Enrollment.Create(studentId, request.TransportGroupId);
        if (result.IsFailure)
        {
            return Result.Failure<EnrollmentResponse>(result.Error);
        }

        var enrollment = result.Value;
        repository.Add(enrollment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new EnrollmentResponse(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.TransportGroupId,
            groupName,
            enrollment.Active,
            enrollment.StartedAtUtc,
            enrollment.EndedAtUtc));
    }

    public async Task<Result> EndAsync(
        Guid transporterId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (enrollment is null)
        {
            return Result.Failure(Error.NotFound("Enrollment.NotFound", "Enrollment not found."));
        }

        enrollment.End();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
