using AttendanceManagement.Application.GuardianConsents;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Abstractions;

public interface IGuardianConsentRepository
{
    void Add(GuardianConsent consent);

    /// <summary>O consentimento ativo do par (no máximo um), rastreado para revogar.</summary>
    Task<GuardianConsent?> GetActiveAsync(Guid guardianId, Guid studentId, CancellationToken cancellationToken = default);

    Task<bool> AnyActiveAsync(Guid guardianId, Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>Estado do consentimento de cada filho ativamente vinculado ao responsável.</summary>
    Task<IReadOnlyList<GuardianConsentStatusResponse>> ListStatusByGuardianAsync(Guid guardianId, CancellationToken cancellationToken = default);
}
