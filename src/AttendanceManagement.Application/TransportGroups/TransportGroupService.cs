using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.TransportGroups;

/// <summary>Casos de uso do grupo de transporte, sempre escopados ao tenant do chamador.</summary>
public sealed class TransportGroupService(ITransportGroupRepository repository, IUnitOfWork unitOfWork)
{
    public Task<IReadOnlyList<TransportGroupResponse>> ListAsync(
        Guid transporterId,
        CancellationToken cancellationToken = default) =>
        repository.ListAsync(transporterId, cancellationToken);

    public async Task<Result<TransportGroupResponse>> GetByIdAsync(
        Guid transporterId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var group = await repository.GetByIdAsync(transporterId, id, cancellationToken);

        return group is null
            ? Result.Failure<TransportGroupResponse>(Error.NotFound("TransportGroup.NotFound", "Transport group not found."))
            : Result.Success(group);
    }

    public async Task<Result<TransportGroupResponse>> CreateAsync(
        Guid transporterId,
        CreateTransportGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = TransportGroup.Create(transporterId, request.Name, request.Shift);
        if (result.IsFailure)
        {
            return Result.Failure<TransportGroupResponse>(result.Error);
        }

        var group = result.Value;
        repository.Add(group);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(group));
    }

    public async Task<Result<TransportGroupResponse>> UpdateAsync(
        Guid transporterId,
        Guid id,
        UpdateTransportGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var group = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (group is null)
        {
            return Result.Failure<TransportGroupResponse>(Error.NotFound("TransportGroup.NotFound", "Transport group not found."));
        }

        var result = group.Update(request.Name, request.Shift);
        if (result.IsFailure)
        {
            return Result.Failure<TransportGroupResponse>(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(group));
    }

    /// <summary>Designa (ou desfaz) veículo e monitor, validando que ambos pertencem ao tenant.</summary>
    public async Task<Result<TransportGroupResponse>> AssignCrewAsync(
        Guid transporterId,
        Guid id,
        AssignCrewRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var group = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (group is null)
        {
            return Result.Failure<TransportGroupResponse>(Error.NotFound("TransportGroup.NotFound", "Transport group not found."));
        }

        if (request.VehicleId is { } vehicleId && vehicleId != Guid.Empty &&
            !await repository.VehicleExistsAsync(transporterId, vehicleId, cancellationToken))
        {
            return Result.Failure<TransportGroupResponse>(
                Error.NotFound("TransportGroup.VehicleNotFound", "Vehicle not found."));
        }

        if (request.AssistantId is { } assistantId && assistantId != Guid.Empty &&
            !await repository.AssistantExistsAsync(transporterId, assistantId, cancellationToken))
        {
            return Result.Failure<TransportGroupResponse>(
                Error.NotFound("TransportGroup.AssistantNotFound", "Assistant not found."));
        }

        group.AssignCrew(request.VehicleId, request.AssistantId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(group));
    }

    public async Task<Result> DeleteAsync(
        Guid transporterId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var group = await repository.GetForUpdateAsync(transporterId, id, cancellationToken);
        if (group is null)
        {
            return Result.Failure(Error.NotFound("TransportGroup.NotFound", "Transport group not found."));
        }

        if (await repository.HasActiveEnrollmentsAsync(id, cancellationToken))
        {
            return Result.Failure(Error.Conflict(
                "TransportGroup.HasEnrollments",
                "The group still has active enrollments. Move or unenroll the students first."));
        }

        group.SoftDelete();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static TransportGroupResponse ToResponse(TransportGroup group) =>
        new(group.Id, group.Name, group.Shift, group.VehicleId, group.AssistantId);
}
