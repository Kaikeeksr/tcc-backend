using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// A chamada: cabeçalho de uma sessão de um grupo numa data e sentido. No máximo
/// uma por (grupo, data, tipo). <c>VehicleId</c>/<c>AssistantId</c> são snapshot
/// de quem rodou no dia.
/// </summary>
public sealed class AttendanceSession : AuditableEntity
{
    private AttendanceSession(
        Guid id,
        Guid transporterId,
        Guid transportGroupId,
        SessionType sessionType,
        DateOnly sessionDate,
        Guid createdBy,
        Guid? vehicleId,
        Guid? assistantId)
        : base(id)
    {
        TransporterId = transporterId;
        TransportGroupId = transportGroupId;
        SessionType = sessionType;
        SessionDate = sessionDate;
        CreatedBy = createdBy;
        VehicleId = vehicleId;
        AssistantId = assistantId;
        Status = SessionStatus.Open;
        OpenedAtUtc = DateTime.UtcNow;
    }

    private AttendanceSession()
    {
    }

    public Guid TransporterId { get; private set; }

    public Guid TransportGroupId { get; private set; }

    public SessionType SessionType { get; private set; }

    public DateOnly SessionDate { get; private set; }

    public SessionStatus Status { get; private set; }

    public DateTime? OpenedAtUtc { get; private set; }

    public DateTime? ClosedAtUtc { get; private set; }

    public Guid? VehicleId { get; private set; }

    public Guid? AssistantId { get; private set; }

    public Guid CreatedBy { get; private set; }

    public Transporter Transporter { get; private set; } = null!;

    public TransportGroup TransportGroup { get; private set; } = null!;

    public UserAccount CreatedByUser { get; private set; } = null!;

    public bool IsOpen => Status == SessionStatus.Open;

    public static Result<AttendanceSession> Open(
        Guid transporterId,
        Guid transportGroupId,
        SessionType sessionType,
        DateOnly sessionDate,
        Guid createdBy,
        Guid? vehicleId,
        Guid? assistantId)
    {
        if (transporterId == Guid.Empty)
        {
            return Result.Failure<AttendanceSession>(Error.Validation("AttendanceSession.TransporterRequired", "Transporter is required."));
        }

        if (transportGroupId == Guid.Empty)
        {
            return Result.Failure<AttendanceSession>(Error.Validation("AttendanceSession.GroupRequired", "Transport group is required."));
        }

        if (createdBy == Guid.Empty)
        {
            return Result.Failure<AttendanceSession>(Error.Validation("AttendanceSession.CreatedByRequired", "The attendance author is required."));
        }

        return Result.Success(new AttendanceSession(
            Guid.CreateVersion7(),
            transporterId,
            transportGroupId,
            sessionType,
            sessionDate,
            createdBy,
            vehicleId == Guid.Empty ? null : vehicleId,
            assistantId == Guid.Empty ? null : assistantId));
    }

    public Result Close()
    {
        if (Status == SessionStatus.Canceled)
        {
            return Result.Failure(Error.Conflict("AttendanceSession.Canceled", "A canceled attendance cannot be closed."));
        }

        if (Status == SessionStatus.Closed)
        {
            return Result.Failure(Error.Conflict("AttendanceSession.AlreadyClosed", "The attendance is already closed."));
        }

        Status = SessionStatus.Closed;
        ClosedAtUtc = DateTime.UtcNow;
        Touch();
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == SessionStatus.Closed)
        {
            return Result.Failure(Error.Conflict("AttendanceSession.AlreadyClosed", "A closed attendance cannot be canceled."));
        }

        Status = SessionStatus.Canceled;
        ClosedAtUtc = DateTime.UtcNow;
        Touch();
        return Result.Success();
    }
}
