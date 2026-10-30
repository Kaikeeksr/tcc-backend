using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.GuardianConsents;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Tests.Fakes;

public sealed class FakeGuardianConsentRepository : IGuardianConsentRepository
{
    public List<GuardianConsent> Consents { get; } = [];

    /// <summary>Filhos (id, nome) ativamente vinculados a cada responsável, para <see cref="ListStatusByGuardianAsync"/>.</summary>
    public Dictionary<Guid, List<(Guid StudentId, string Name)>> ChildrenByGuardian { get; } = [];

    public void Add(GuardianConsent consent) => Consents.Add(consent);

    public Task<GuardianConsent?> GetActiveAsync(Guid guardianId, Guid studentId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Consents.FirstOrDefault(c =>
            c.GuardianId == guardianId && c.StudentId == studentId && c.RevokedAtUtc is null));

    public Task<bool> AnyActiveAsync(Guid guardianId, Guid studentId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Consents.Any(c => c.GuardianId == guardianId && c.StudentId == studentId && c.RevokedAtUtc is null));

    public Task<IReadOnlyList<GuardianConsentStatusResponse>> ListStatusByGuardianAsync(Guid guardianId, CancellationToken cancellationToken = default)
    {
        var children = ChildrenByGuardian.TryGetValue(guardianId, out var list) ? list : [];

        var statuses = children.Select(child =>
        {
            var latest = Consents
                .Where(c => c.GuardianId == guardianId && c.StudentId == child.StudentId)
                .OrderByDescending(c => c.GrantedAtUtc)
                .FirstOrDefault();

            return new GuardianConsentStatusResponse(
                child.StudentId,
                child.Name,
                latest is not null && latest.RevokedAtUtc is null,
                latest?.TermsVersion,
                latest?.GrantedAtUtc,
                latest?.RevokedAtUtc);
        }).ToList();

        return Task.FromResult<IReadOnlyList<GuardianConsentStatusResponse>>(statuses);
    }
}
