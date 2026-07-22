using AttendanceManagement.Domain.Abstractions;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// Vínculo N:N student ↔ transport_group. Hoje há uma matrícula ativa por
/// aluno, mas o modelo já é N:N para permitir múltiplos grupos sem quebra.
/// Temporal: trocar de grupo encerra a matrícula antiga em vez de apagá-la.
/// </summary>
public sealed class Enrollment : AuditableEntity
{
    private Enrollment(Guid id, Guid studentId, Guid transportGroupId)
        : base(id)
    {
        StudentId = studentId;
        TransportGroupId = transportGroupId;
        Active = true;
        StartedAtUtc = DateTime.UtcNow;
    }

    private Enrollment()
    {
    }

    public Guid StudentId { get; private set; }

    public Guid TransportGroupId { get; private set; }

    public bool Active { get; private set; }

    public DateTime StartedAtUtc { get; private set; }

    public DateTime? EndedAtUtc { get; private set; }

    public static Result<Enrollment> Create(Guid studentId, Guid transportGroupId)
    {
        if (studentId == Guid.Empty)
        {
            return Result.Failure<Enrollment>(Error.Validation("Enrollment.StudentRequired", "O aluno é obrigatório."));
        }

        if (transportGroupId == Guid.Empty)
        {
            return Result.Failure<Enrollment>(Error.Validation("Enrollment.GroupRequired", "O grupo é obrigatório."));
        }

        return Result.Success(new Enrollment(Guid.CreateVersion7(), studentId, transportGroupId));
    }

    public void End()
    {
        if (!Active)
        {
            return;
        }

        Active = false;
        EndedAtUtc = DateTime.UtcNow;
        Touch();
    }
}
