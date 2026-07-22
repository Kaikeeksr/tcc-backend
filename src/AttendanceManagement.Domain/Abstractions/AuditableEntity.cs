namespace AttendanceManagement.Domain.Abstractions;

/// <summary>
/// Entidade com carimbos de tempo de auditoria, sempre em UTC.
/// `CreatedAtUtc` é fixado na criação; `UpdatedAtUtc` avança a cada mutação
/// via <see cref="Touch"/>.
/// </summary>
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
