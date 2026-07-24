using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// Identidade e credencial. Único ponto de autenticação: transporter, assistant,
/// guardian e (opcionalmente) student apontam para cá. Só credencial — dado de
/// negócio fica nos perfis.
/// </summary>
public sealed class UserAccount : AuditableEntity
{
    public const int EmailMaxLength = 254;

    private UserAccount(Guid id, string email, string? phone, string passwordHash, PrimaryRole primaryRole)
        : base(id)
    {
        Email = email;
        Phone = phone;
        PasswordHash = passwordHash;
        PrimaryRole = primaryRole;
        Status = UserStatus.Pending;
    }

    private UserAccount()
    {
    }

    public string Email { get; private set; } = null!;

    public string? Phone { get; private set; }

    public string PasswordHash { get; private set; } = null!;

    public UserStatus Status { get; private set; }

    public PrimaryRole PrimaryRole { get; private set; }

    public DateTime? EmailConfirmedAtUtc { get; private set; }

    /// <summary>O hash já vem calculado — o domínio nunca vê a senha em texto puro.</summary>
    public static Result<UserAccount> Create(
        string? email,
        string? phone,
        string? passwordHash,
        PrimaryRole primaryRole)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<UserAccount>(Error.Validation("UserAccount.EmailRequired", "Email is required."));
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (normalizedEmail.Length > EmailMaxLength)
        {
            return Result.Failure<UserAccount>(Error.Validation("UserAccount.EmailTooLong", "Email exceeds the maximum length."));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return Result.Failure<UserAccount>(Error.Validation("UserAccount.PasswordRequired", "The credential is required."));
        }

        return Result.Success(new UserAccount(
            Guid.CreateVersion7(),
            normalizedEmail,
            string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            passwordHash,
            primaryRole));
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        EmailConfirmedAtUtc ??= DateTime.UtcNow;
        Touch();
    }

    public void Block()
    {
        Status = UserStatus.Blocked;
        Touch();
    }
}
