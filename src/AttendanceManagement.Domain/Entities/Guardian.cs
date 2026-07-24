using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.ValueObjects;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// Responsável (pai/mãe/avó/tutor). Concentra os dados pessoais de contato — o
/// centro de gravidade da LGPD. Login obrigatório.
/// </summary>
public sealed class Guardian : SoftDeletableEntity
{
    public const int NameMaxLength = 150;

    private Guardian(Guid id, Guid transporterId, Guid userAccountId, string name, Address address)
        : base(id)
    {
        TransporterId = transporterId;
        UserAccountId = userAccountId;
        Name = name;
        Address = address;
    }

    private Guardian()
    {
    }

    public Guid TransporterId { get; private set; }

    public Guid UserAccountId { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Phone { get; private set; }

    public string? Mobile { get; private set; }

    public string? Whatsapp { get; private set; }

    public string? ContactEmail { get; private set; }

    public Address Address { get; private set; } = null!;

    public Transporter Transporter { get; private set; } = null!;

    public UserAccount UserAccount { get; private set; } = null!;

    public static Result<Guardian> Create(
        Guid transporterId,
        Guid userAccountId,
        string? name,
        Address? address)
    {
        if (transporterId == Guid.Empty)
        {
            return Result.Failure<Guardian>(Error.Validation("Guardian.TransporterRequired", "Transporter is required."));
        }

        if (userAccountId == Guid.Empty)
        {
            return Result.Failure<Guardian>(Error.Validation("Guardian.UserRequired", "The login account is required (a guardian always logs in)."));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Guardian>(Error.Validation("Guardian.NameRequired", "Name is required."));
        }

        return Result.Success(new Guardian(
            Guid.CreateVersion7(),
            transporterId,
            userAccountId,
            name.Trim(),
            address ?? Address.Empty()));
    }

    public void SetContact(string? phone, string? mobile, string? whatsapp, string? contactEmail)
    {
        Phone = Normalize(phone);
        Mobile = Normalize(mobile);
        Whatsapp = Normalize(whatsapp);
        ContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim().ToLowerInvariant();
        Touch();
    }

    public void SetAddress(Address address)
    {
        Address = address;
        Touch();
    }

    public Result Rename(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Error.Validation("Guardian.NameRequired", "Name is required."));
        }

        Name = name.Trim();
        Touch();
        return Result.Success();
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
