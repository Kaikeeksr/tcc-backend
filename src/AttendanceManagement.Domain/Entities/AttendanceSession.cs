using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// A chamada: cabeçalho de uma sessão de um grupo em uma data e sentido.
/// No máximo uma por (grupo, data, tipo).
///
/// `VehicleId`/`AssistantId` são snapshot de quem rodou no dia — o histórico
/// não muda se a designação do grupo mudar depois.
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
            return Result.Failure<AttendanceSession>(Error.Validation("AttendanceSession.TransporterRequired", "O transportador é obrigatório."));
        }

        if (transportGroupId == Guid.Empty)
        {
            return Result.Failure<AttendanceSession>(Error.Validation("AttendanceSession.GroupRequired", "O grupo é obrigatório."));
        }

        if (createdBy == Guid.Empty)
        {
            return Result.Failure<AttendanceSession>(Error.Validation("AttendanceSession.CreatedByRequired", "O autor da chamada é obrigatório."));
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
            return Result.Failure(Error.Conflict("AttendanceSession.Canceled", "Uma chamada cancelada não pode ser fechada."));
        }

        if (Status == SessionStatus.Closed)
        {
            return Result.Failure(Error.Conflict("AttendanceSession.AlreadyClosed", "A chamada já está fechada."));
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
            return Result.Failure(Error.Conflict("AttendanceSession.AlreadyClosed", "Uma chamada fechada não pode ser cancelada."));
        }

        Status = SessionStatus.Canceled;
        ClosedAtUtc = DateTime.UtcNow;
        Touch();
        return Result.Success();
    }
}
