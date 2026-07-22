using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.ValueObjects;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// Responsável (pai/mãe/avó/tutor). Concentra os dados pessoais de contato —
/// é o centro de gravidade da LGPD, e por isso a anonimização de erasure toca
/// poucas tabelas. Login obrigatório.
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

    /// <summary>Canal de contato — pode diferir do e-mail de login.</summary>
    public string? ContactEmail { get; private set; }

    public Address Address { get; private set; } = null!;

    public static Result<Guardian> Create(
        Guid transporterId,
        Guid userAccountId,
        string? name,
        Address? address)
    {
        if (transporterId == Guid.Empty)
        {
            return Result.Failure<Guardian>(Error.Validation("Guardian.TransporterRequired", "O transportador é obrigatório."));
        }

        if (userAccountId == Guid.Empty)
        {
            return Result.Failure<Guardian>(Error.Validation("Guardian.UserRequired", "A conta de login é obrigatória (responsável sempre loga)."));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Guardian>(Error.Validation("Guardian.NameRequired", "O nome é obrigatório."));
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

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
