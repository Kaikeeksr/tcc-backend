using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// O "tio da van" — usuário principal e raiz do tenant. Todo dado do sistema
/// pertence a exatamente um transporter.
/// </summary>
public sealed class Transporter : SoftDeletableEntity
{
    public const int NameMaxLength = 150;

    private Transporter(
        Guid id,
        Guid userAccountId,
        string name,
        DocumentType documentType,
        string documentNumber)
        : base(id)
    {
        UserAccountId = userAccountId;
        Name = name;
        DocumentType = documentType;
        DocumentNumber = documentNumber;
    }

    private Transporter()
    {
    }

    public Guid UserAccountId { get; private set; }

    public string Name { get; private set; } = null!;

    public DocumentType DocumentType { get; private set; }

    /// <summary>CPF ou CNPJ, só dígitos.</summary>
    public string DocumentNumber { get; private set; } = null!;

    public string? LicenseNumber { get; private set; }

    public string? LicenseCategory { get; private set; }

    public DateOnly? LicenseExpiry { get; private set; }

    public string? BusinessPhone { get; private set; }

    public UserAccount UserAccount { get; private set; } = null!;

    public static Result<Transporter> Create(
        Guid userAccountId,
        string? name,
        DocumentType documentType,
        string? documentNumber)
    {
        if (userAccountId == Guid.Empty)
        {
            return Result.Failure<Transporter>(Error.Validation("Transporter.UserRequired", "The login account is required."));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Transporter>(Error.Validation("Transporter.NameRequired", "Name is required."));
        }

        var digits = new string((documentNumber ?? string.Empty).Where(char.IsDigit).ToArray());

        if (digits.Length == 0)
        {
            return Result.Failure<Transporter>(Error.Validation("Transporter.DocumentRequired", "The CPF/CNPJ is required."));
        }

        return Result.Success(new Transporter(
            Guid.CreateVersion7(),
            userAccountId,
            name.Trim(),
            documentType,
            digits));
    }

    public void SetDriverLicense(string? number, string? category, DateOnly? expiry)
    {
        LicenseNumber = string.IsNullOrWhiteSpace(number) ? null : number.Trim();
        LicenseCategory = string.IsNullOrWhiteSpace(category) ? null : category.Trim().ToUpperInvariant();
        LicenseExpiry = expiry;
        Touch();
    }
}
