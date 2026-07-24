namespace AttendanceManagement.Domain.Abstractions;

/// <summary>
/// Entidade com soft delete: um filtro global do EF Core esconde as linhas com
/// <see cref="DeletedAtUtc"/> preenchido. Não é erasure de LGPD — só some das listas.
/// </summary>
public abstract class SoftDeletableEntity : AuditableEntity
{
    protected SoftDeletableEntity(Guid id)
        : base(id)
    {
    }

    protected SoftDeletableEntity()
    {
    }

    public DateTime? DeletedAtUtc { get; protected set; }

    public bool IsDeleted => DeletedAtUtc.HasValue;

    public void SoftDelete()
    {
        if (IsDeleted)
        {
            return;
        }

        DeletedAtUtc = DateTime.UtcNow;
        Touch();
    }

    public void Restore()
    {
        DeletedAtUtc = null;
        Touch();
    }
}
