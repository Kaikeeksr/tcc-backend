using AttendanceManagement.Domain.Abstractions;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// O monitor da van: login próprio, pertencente a EXATAMENTE UM transporter.
/// Não tem permissão própria — enxerga as mesmas turmas do transporter porque
/// compartilha o `transporter_id`.
/// </summary>
public sealed class Assistant : SoftDeletableEntity
{
    private Assistant(Guid id, Guid transporterId, Guid userAccountId, string name)
        : base(id)
    {
        TransporterId = transporterId;
        UserAccountId = userAccountId;
        Name = name;
    }

    private Assistant()
    {
    }

    public Guid TransporterId { get; private set; }

    public Guid UserAccountId { get; private set; }

    public string Name { get; private set; } = null!;

    public static Result<Assistant> Create(Guid transporterId, Guid userAccountId, string? name)
    {
        if (transporterId == Guid.Empty)
        {
            return Result.Failure<Assistant>(Error.Validation("Assistant.TransporterRequired", "O transportador é obrigatório."));
        }

        if (userAccountId == Guid.Empty)
        {
            return Result.Failure<Assistant>(Error.Validation("Assistant.UserRequired", "A conta de login é obrigatória."));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Assistant>(Error.Validation("Assistant.NameRequired", "O nome é obrigatório."));
        }

        return Result.Success(new Assistant(Guid.CreateVersion7(), transporterId, userAccountId, name.Trim()));
    }
}
