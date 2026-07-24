using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.Common;
using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Schools;

/// <summary>Casos de uso da escola, sempre escopados ao tenant do chamador.</summary>
public sealed class SchoolService(ISchoolRepository repository, IUnitOfWork unitOfWork)
{
    public Task<IReadOnlyList<SchoolResponse>> ListAsync(
        Guid transporterId,
        CancellationToken cancellationToken = default) =>
        repository.ListAsync(transporterId, cancellationToken);

    public async Task<Result<SchoolResponse>> GetByIdAsync(
        Guid transporterId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var school = await repository.GetByIdAsync(transporterId, id, cancellationToken);

        return school is null
            ? Result.Failure<SchoolResponse>(Error.NotFound("School.NotFound", "School not found."))
            : Result.Success(school);
    }

    public async Task<Result<SchoolResponse>> CreateAsync(
        Guid transporterId,
        CreateSchoolRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = School.Create(transporterId, request.Name, request.Address?.ToValueObject());
        if (result.IsFailure)
        {
            return Result.Failure<SchoolResponse>(result.Error);
        }

        var school = result.Value;

        if (await repository.NameExistsAsync(transporterId, school.Name, null, cancellationToken))
        {
            return Result.Failure<SchoolResponse>(
                Error.Conflict("School.NameInUse", "A school with this name already exists."));
        }

        repository.Add(school);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(school));
    }

    public async Task<Result<SchoolResponse>> UpdateAsync(
        Guid transporterId,
        Guid id,
        UpdateSchoolRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var school = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (school is null)
        {
            return Result.Failure<SchoolResponse>(Error.NotFound("School.NotFound", "School not found."));
        }

        var result = school.Update(request.Name, request.Address?.ToValueObject());
        if (result.IsFailure)
        {
            return Result.Failure<SchoolResponse>(result.Error);
        }

        if (await repository.NameExistsAsync(transporterId, school.Name, id, cancellationToken))
        {
            return Result.Failure<SchoolResponse>(
                Error.Conflict("School.NameInUse", "A school with this name already exists."));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(school));
    }

    public async Task<Result> DeleteAsync(
        Guid transporterId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var school = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (school is null)
        {
            return Result.Failure(Error.NotFound("School.NotFound", "School not found."));
        }

        school.SoftDelete();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static SchoolResponse ToResponse(School school) =>
        new(school.Id, school.Name, AddressPayload.FromValueObject(school.Address));
}
