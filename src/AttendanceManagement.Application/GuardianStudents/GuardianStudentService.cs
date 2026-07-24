using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.GuardianStudents;

/// <summary>
/// Casos de uso do vínculo responsável↔aluno: vincular, atualizar flags e desvincular.
/// Ao marcar um contato como principal, rebaixa o principal anterior antes de gravar
/// (o índice único só admite um principal ativo por aluno).
/// </summary>
public sealed class GuardianStudentService(IGuardianStudentRepository repository, IUnitOfWork unitOfWork)
{
    public async Task<Result<IReadOnlyList<GuardianStudentResponse>>> ListByStudentAsync(
        Guid transporterId,
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        if (!await repository.StudentExistsAsync(transporterId, studentId, cancellationToken))
        {
            return Result.Failure<IReadOnlyList<GuardianStudentResponse>>(
                Error.NotFound("Student.NotFound", "Student not found."));
        }

        return Result.Success(await repository.ListByStudentAsync(studentId, cancellationToken));
    }

    public async Task<Result<GuardianStudentResponse>> LinkAsync(
        Guid transporterId,
        Guid studentId,
        LinkGuardianRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await repository.StudentExistsAsync(transporterId, studentId, cancellationToken))
        {
            return Result.Failure<GuardianStudentResponse>(Error.NotFound("Student.NotFound", "Student not found."));
        }

        if (!await repository.GuardianExistsAsync(transporterId, request.GuardianId, cancellationToken))
        {
            return Result.Failure<GuardianStudentResponse>(
                Error.NotFound("GuardianStudent.GuardianNotFound", "Guardian not found."));
        }

        if (await repository.ActivePairExistsAsync(request.GuardianId, studentId, cancellationToken))
        {
            return Result.Failure<GuardianStudentResponse>(
                Error.Conflict("GuardianStudent.AlreadyLinked", "This guardian is already linked to the student."));
        }

        if (request.IsPrimary)
        {
            await DemoteActivePrimaryAsync(studentId, null, cancellationToken);
        }

        var result = GuardianStudent.Create(
            request.GuardianId,
            studentId,
            request.Relationship,
            request.IsPrimary,
            request.CanPickup);

        if (result.IsFailure)
        {
            return Result.Failure<GuardianStudentResponse>(result.Error);
        }

        var link = result.Value;
        repository.Add(link);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(link));
    }

    public async Task<Result<GuardianStudentResponse>> UpdateAsync(
        Guid transporterId,
        Guid id,
        UpdateGuardianStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var link = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (link is null)
        {
            return Result.Failure<GuardianStudentResponse>(
                Error.NotFound("GuardianStudent.NotFound", "Guardian-student link not found."));
        }

        if (!link.Active)
        {
            return Result.Failure<GuardianStudentResponse>(
                Error.Conflict("GuardianStudent.Ended", "An ended link cannot be updated."));
        }

        if (request.IsPrimary)
        {
            await DemoteActivePrimaryAsync(link.StudentId, id, cancellationToken);
        }

        link.SetRelationship(request.Relationship);
        link.SetCanPickup(request.CanPickup);
        link.SetPrimary(request.IsPrimary);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(link));
    }

    public async Task<Result> EndAsync(
        Guid transporterId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var link = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (link is null)
        {
            return Result.Failure(Error.NotFound("GuardianStudent.NotFound", "Guardian-student link not found."));
        }

        link.End();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    // Rebaixa o principal atual numa gravação própria, antes da promoção do novo,
    // para nunca haver dois principais ativos ao mesmo tempo (o índice é imediato).
    private async Task DemoteActivePrimaryAsync(Guid studentId, Guid? excludeId, CancellationToken cancellationToken)
    {
        var current = await repository.GetActivePrimaryAsync(studentId, excludeId, cancellationToken);
        if (current is null)
        {
            return;
        }

        current.SetPrimary(false);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static GuardianStudentResponse ToResponse(GuardianStudent link) =>
        new(link.Id, link.GuardianId, link.StudentId, link.Relationship, link.IsPrimary, link.CanPickup, link.Active);
}
