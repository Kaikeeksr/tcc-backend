using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// Vínculo N:N guardian ↔ student. <c>CanPickup</c> alimenta o "retirado pelo
/// responsável"; <c>IsPrimary</c> marca o contato principal (no máximo um ativo
/// por aluno). Temporal.
/// </summary>
public sealed class GuardianStudent : AuditableEntity
{
    private GuardianStudent(
        Guid id,
        Guid guardianId,
        Guid studentId,
        RelationshipType relationship,
        bool isPrimary,
        bool canPickup)
        : base(id)
    {
        GuardianId = guardianId;
        StudentId = studentId;
        Relationship = relationship;
        IsPrimary = isPrimary;
        CanPickup = canPickup;
        Active = true;
        StartedAtUtc = DateTime.UtcNow;
    }

    private GuardianStudent()
    {
    }

    public Guid GuardianId { get; private set; }

    public Guid StudentId { get; private set; }

    public RelationshipType Relationship { get; private set; }

    public bool IsPrimary { get; private set; }

    public bool CanPickup { get; private set; }

    public bool Active { get; private set; }

    public DateTime StartedAtUtc { get; private set; }

    public DateTime? EndedAtUtc { get; private set; }

    public Guardian Guardian { get; private set; } = null!;

    public Student Student { get; private set; } = null!;

    public static Result<GuardianStudent> Create(
        Guid guardianId,
        Guid studentId,
        RelationshipType relationship,
        bool isPrimary,
        bool canPickup)
    {
        if (guardianId == Guid.Empty)
        {
            return Result.Failure<GuardianStudent>(Error.Validation("GuardianStudent.GuardianRequired", "Guardian is required."));
        }

        if (studentId == Guid.Empty)
        {
            return Result.Failure<GuardianStudent>(Error.Validation("GuardianStudent.StudentRequired", "Student is required."));
        }

        return Result.Success(new GuardianStudent(
            Guid.CreateVersion7(),
            guardianId,
            studentId,
            relationship,
            isPrimary,
            canPickup));
    }

    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
        Touch();
    }

    public void SetRelationship(RelationshipType relationship)
    {
        Relationship = relationship;
        Touch();
    }

    public void SetCanPickup(bool canPickup)
    {
        CanPickup = canPickup;
        Touch();
    }

    public void End()
    {
        if (!Active)
        {
            return;
        }

        Active = false;
        IsPrimary = false;
        EndedAtUtc = DateTime.UtcNow;
        Touch();
    }
}
