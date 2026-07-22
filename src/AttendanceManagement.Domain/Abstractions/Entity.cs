namespace AttendanceManagement.Domain.Abstractions;

/// <summary>
/// Base de toda entidade do dominio. Carrega apenas a identidade.
/// Nao herda de nada do EF Core: o dominio nao sabe que persistencia existe.
/// </summary>
public abstract class Entity
{
    protected Entity(Guid id) => Id = id;

    /// <summary>Construtor sem parametros exigido pelo materializador do EF Core.</summary>
    protected Entity()
    {
    }

    public Guid Id { get; protected set; }
}
