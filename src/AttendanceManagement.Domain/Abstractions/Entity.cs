namespace AttendanceManagement.Domain.Abstractions;

public abstract class Entity
{
    protected Entity(Guid id) => Id = id;

    // Exigido pelo materializador do EF Core.
    protected Entity()
    {
    }

    public Guid Id { get; protected set; }
}
