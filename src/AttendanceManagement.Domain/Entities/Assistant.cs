using AttendanceManagement.Domain.Abstractions;

namespace AttendanceManagement.Domain.Entities;

/// <summary>O monitor da van: login próprio, pertencente a exatamente um transporter.</summary>
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

    public Transporter Transporter { get; private set; } = null!;

    public UserAccount UserAccount { get; private set; } = null!;

    public static Result<Assistant> Create(Guid transporterId, Guid userAccountId, string? name)
    {
        if (transporterId == Guid.Empty)
        {
            return Result.Failure<Assistant>(Error.Validation("Assistant.TransporterRequired", "Transporter is required."));
        }

        if (userAccountId == Guid.Empty)
        {
            return Result.Failure<Assistant>(Error.Validation("Assistant.UserRequired", "The login account is required."));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Assistant>(Error.Validation("Assistant.NameRequired", "Name is required."));
        }

        return Result.Success(new Assistant(Guid.CreateVersion7(), transporterId, userAccountId, name.Trim()));
    }

    public Result Rename(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Error.Validation("Assistant.NameRequired", "Name is required."));
        }

        Name = name.Trim();
        Touch();
        return Result.Success();
    }
}
