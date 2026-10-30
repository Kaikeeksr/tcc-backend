using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.GuardianConsents;
using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Persistence.Repositories;

internal sealed class GuardianConsentRepository(AppDbContext context) : IGuardianConsentRepository
{
    public void Add(GuardianConsent consent)
    {
        ArgumentNullException.ThrowIfNull(consent);

        context.GuardianConsents.Add(consent);
    }

    public Task<GuardianConsent?> GetActiveAsync(Guid guardianId, Guid studentId, CancellationToken cancellationToken = default) =>
        context.GuardianConsents
            .AsTracking()
            .Where(c => c.GuardianId == guardianId && c.StudentId == studentId && c.RevokedAtUtc == null)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> AnyActiveAsync(Guid guardianId, Guid studentId, CancellationToken cancellationToken = default) =>
        context.GuardianConsents.AnyAsync(
            c => c.GuardianId == guardianId && c.StudentId == studentId && c.RevokedAtUtc == null,
            cancellationToken);

    public async Task<IReadOnlyList<GuardianConsentStatusResponse>> ListStatusByGuardianAsync(Guid guardianId, CancellationToken cancellationToken = default) =>
        await context.GuardianStudents
            .Where(gs => gs.GuardianId == guardianId && gs.Active)
            .OrderBy(gs => gs.Student.Name)
            .Select(gs => new
            {
                gs.StudentId,
                gs.Student.Name,
                LatestConsent = context.GuardianConsents
                    .Where(c => c.GuardianId == guardianId && c.StudentId == gs.StudentId)
                    .OrderByDescending(c => c.GrantedAtUtc)
                    .FirstOrDefault(),
            })
            .Select(x => new GuardianConsentStatusResponse(
                x.StudentId,
                x.Name,
                x.LatestConsent != null && x.LatestConsent.RevokedAtUtc == null,
                x.LatestConsent == null ? null : x.LatestConsent.TermsVersion,
                x.LatestConsent == null ? null : x.LatestConsent.GrantedAtUtc,
                x.LatestConsent == null ? null : x.LatestConsent.RevokedAtUtc))
            .ToListAsync(cancellationToken);
}
