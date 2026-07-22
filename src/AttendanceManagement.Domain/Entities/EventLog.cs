using AttendanceManagement.Domain.Abstractions;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// Trilha append-only de eventos de domínio: nunca sofre update nem delete.
/// É o mecanismo de extensão — um evento novo é só um novo valor de
/// <see cref="EventType"/>, sem migration.
/// </summary>
public sealed class EventLog : Entity
{
    private EventLog(
        Guid id,
        Guid transporterId,
        string eventType,
        string aggregateType,
        Guid aggregateId,
        Guid? actorUserId,
        string payload)
        : base(id)
    {
        TransporterId = transporterId;
        EventType = eventType;
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        ActorUserId = actorUserId;
        Payload = payload;
        OccurredAtUtc = DateTime.UtcNow;
    }

    private EventLog()
    {
    }

    public Guid TransporterId { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    /// <summary>Ex.: "AttendanceRecord.PickedUpByGuardian".</summary>
    public string EventType { get; private set; } = null!;

    public string AggregateType { get; private set; } = null!;

    public Guid AggregateId { get; private set; }

    public Guid? ActorUserId { get; private set; }

    /// <summary>Detalhes em JSON (coluna jsonb).</summary>
    public string Payload { get; private set; } = null!;

    public static EventLog Record(
        Guid transporterId,
        string eventType,
        string aggregateType,
        Guid aggregateId,
        Guid? actorUserId,
        string? payloadJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(aggregateType);

        return new EventLog(
            Guid.CreateVersion7(),
            transporterId,
            eventType.Trim(),
            aggregateType.Trim(),
            aggregateId,
            actorUserId == Guid.Empty ? null : actorUserId,
            string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson);
    }
}
