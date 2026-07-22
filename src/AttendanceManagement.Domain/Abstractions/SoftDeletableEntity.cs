namespace AttendanceManagement.Domain.Abstractions;

/// <summary>
/// Entidade de domínio com soft delete: em vez de sumir do banco, ganha um
/// `DeletedAtUtc`. Um filtro global do EF Core (`HasQueryFilter`) esconde as
/// linhas apagadas das consultas por padrão.
///
/// Soft delete NÃO é erasure de LGPD — é só "sumir das listas". A remoção de
/// dado pessoal (direito ao esquecimento) é uma rotina de anonimização à parte.
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
