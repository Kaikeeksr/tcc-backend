using AttendanceManagement.Domain.Abstractions;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// Consentimento do responsável para o tratamento de dados do filho (LGPD, art. 14,
/// §1º). Nunca é apagado nem sobrescrito: revogar marca <see cref="RevokedAtUtc"/> na
/// própria linha, e conceder de novo cria uma linha nova — o histórico completo
/// (quem concedeu, quando, em qual versão do termo, e quando revogou) permanece.
/// </summary>
public sealed class GuardianConsent : Entity
{
    private GuardianConsent(
        Guid id,
        Guid transporterId,
        Guid guardianId,
        Guid studentId,
        string termsVersion)
        : base(id)
    {
        TransporterId = transporterId;
        GuardianId = guardianId;
        StudentId = studentId;
        TermsVersion = termsVersion;
        GrantedAtUtc = DateTime.UtcNow;
    }

    private GuardianConsent()
    {
    }

    public Guid TransporterId { get; private set; }

    public Guid GuardianId { get; private set; }

    public Guid StudentId { get; private set; }

    public string TermsVersion { get; private set; } = null!;

    public DateTime GrantedAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public bool IsActive => RevokedAtUtc is null;

    public Guardian Guardian { get; private set; } = null!;

    public Student Student { get; private set; } = null!;

    public static Result<GuardianConsent> Create(
        Guid transporterId,
        Guid guardianId,
        Guid studentId,
        string? termsVersion)
    {
        if (guardianId == Guid.Empty)
        {
            return Result.Failure<GuardianConsent>(Error.Validation("GuardianConsent.GuardianRequired", "Guardian is required."));
        }

        if (studentId == Guid.Empty)
        {
            return Result.Failure<GuardianConsent>(Error.Validation("GuardianConsent.StudentRequired", "Student is required."));
        }

        if (string.IsNullOrWhiteSpace(termsVersion))
        {
            return Result.Failure<GuardianConsent>(Error.Validation("GuardianConsent.TermsVersionRequired", "Terms version is required."));
        }

        return Result.Success(new GuardianConsent(
            Guid.CreateVersion7(),
            transporterId,
            guardianId,
            studentId,
            termsVersion.Trim()));
    }

    public Result Revoke()
    {
        if (!IsActive)
        {
            return Result.Failure(Error.Conflict("GuardianConsent.AlreadyRevoked", "This consent was already revoked."));
        }

        RevokedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }
}
