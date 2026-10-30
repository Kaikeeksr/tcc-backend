using System.Text.Json;
using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.GuardianConsents;

/// <summary>
/// Casos de uso do consentimento LGPD (art. 14, §1º): conceder e revogar, sempre
/// condicionado a um vínculo responsável↔aluno ativo. Cada concessão e revogação
/// vai para o <see cref="EventLog"/>.
/// </summary>
public sealed class GuardianConsentService(
    IGuardianConsentRepository consents,
    IGuardianStudentRepository guardianStudents,
    IEventLogRepository eventLog,
    IUnitOfWork unitOfWork)
{
    public Task<IReadOnlyList<GuardianConsentStatusResponse>> ListByGuardianAsync(
        Guid guardianId,
        CancellationToken cancellationToken = default) =>
        consents.ListStatusByGuardianAsync(guardianId, cancellationToken);

    public async Task<Result<GuardianConsentStatusResponse>> GrantAsync(
        Guid transporterId,
        Guid guardianId,
        Guid studentId,
        Guid actorUserId,
        GrantGuardianConsentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await guardianStudents.ActivePairExistsAsync(guardianId, studentId, cancellationToken))
        {
            return Result.Failure<GuardianConsentStatusResponse>(
                Error.Unauthorized("GuardianConsent.NotLinked", "This student is not linked to your account."));
        }

        if (await consents.AnyActiveAsync(guardianId, studentId, cancellationToken))
        {
            return Result.Failure<GuardianConsentStatusResponse>(
                Error.Conflict("GuardianConsent.AlreadyActive", "Consent for this student is already active."));
        }

        var result = GuardianConsent.Create(transporterId, guardianId, studentId, request.TermsVersion);
        if (result.IsFailure)
        {
            return Result.Failure<GuardianConsentStatusResponse>(result.Error);
        }

        var consent = result.Value;
        consents.Add(consent);

        eventLog.Add(EventLog.Record(
            transporterId,
            "GuardianConsent.Granted",
            nameof(GuardianConsent),
            consent.Id,
            actorUserId,
            JsonSerializer.Serialize(new { guardianId, studentId, termsVersion = consent.TermsVersion })));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(await ToStatusAsync(consent, cancellationToken));
    }

    public async Task<Result> RevokeAsync(
        Guid transporterId,
        Guid guardianId,
        Guid studentId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var consent = await consents.GetActiveAsync(guardianId, studentId, cancellationToken);
        if (consent is null)
        {
            return Result.Failure(Error.Conflict("GuardianConsent.NotActive", "There is no active consent for this student."));
        }

        var revokeResult = consent.Revoke();
        if (revokeResult.IsFailure)
        {
            return revokeResult;
        }

        eventLog.Add(EventLog.Record(
            transporterId,
            "GuardianConsent.Revoked",
            nameof(GuardianConsent),
            consent.Id,
            actorUserId,
            JsonSerializer.Serialize(new { guardianId, studentId })));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<GuardianConsentStatusResponse> ToStatusAsync(GuardianConsent consent, CancellationToken cancellationToken)
    {
        var status = await consents.ListStatusByGuardianAsync(consent.GuardianId, cancellationToken);
        return status.First(s => s.StudentId == consent.StudentId);
    }
}
