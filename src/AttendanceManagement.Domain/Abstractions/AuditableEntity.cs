namespace AttendanceManagement.Domain.Abstractions;

public abstract class AuditableEntity : Entity
{
    protected AuditableEntity(Guid id)
        : base(id)
    {
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    protected AuditableEntity()
    {
    }

    public DateTime CreatedAtUtc { get; protected set; }

    public DateTime UpdatedAtUtc { get; protected set; }

    protected void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
