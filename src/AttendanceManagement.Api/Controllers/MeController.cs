using AttendanceManagement.Application.Attendance;
using AttendanceManagement.Application.GuardianConsents;
using AttendanceManagement.Application.GuardianStudents;
using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Api.Controllers;

/// <summary>
/// Autoatendimento do responsável (meus filhos) e do aluno (minha frequência).
/// Cada ação valida o vínculo/identidade além do escopo de tenant.
/// </summary>
[Route("api/me")]
[Authorize]
public sealed class MeController(
    GuardianStudentService guardianStudents,
    AttendanceService attendance,
    GuardianConsentService consents) : ApiController
{
    /// <summary>Lista os filhos ativamente vinculados ao responsável autenticado.</summary>
    [HttpGet("children")]
    [Authorize(Roles = nameof(PrimaryRole.Guardian))]
    [ProducesResponseType<IReadOnlyList<MyChildResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChildren(CancellationToken cancellationToken) =>
        Ok(await guardianStudents.ListByGuardianAsync(ProfileId, cancellationToken));

    /// <summary>Histórico e frequência de um filho, se o responsável autenticado estiver ativamente vinculado a ele.</summary>
    [HttpGet("children/{studentId:guid}/attendance")]
    [Authorize(Roles = nameof(PrimaryRole.Guardian))]
    [ProducesResponseType<StudentAttendanceHistory>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChildAttendance(
        Guid studentId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken)
    {
        if (!await guardianStudents.IsActivelyLinkedAsync(ProfileId, studentId, cancellationToken))
        {
            return ToProblem(Error.Unauthorized("Me.NotLinked", "This student is not linked to your account."));
        }

        return FromResult(await attendance.GetStudentHistoryAsync(TenantId, studentId, from, to, cancellationToken));
    }

    /// <summary>Histórico e frequência do próprio aluno autenticado.</summary>
    [HttpGet("attendance")]
    [Authorize(Roles = nameof(PrimaryRole.Student))]
    [ProducesResponseType<StudentAttendanceHistory>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyAttendance(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken) =>
        FromResult(await attendance.GetStudentHistoryAsync(TenantId, ProfileId, from, to, cancellationToken));

    /// <summary>Estado do consentimento LGPD de cada filho ativamente vinculado ao responsável autenticado.</summary>
    [HttpGet("consents")]
    [Authorize(Roles = nameof(PrimaryRole.Guardian))]
    [ProducesResponseType<IReadOnlyList<GuardianConsentStatusResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConsents(CancellationToken cancellationToken) =>
        Ok(await consents.ListByGuardianAsync(ProfileId, cancellationToken));

    /// <summary>Concede o consentimento LGPD para o tratamento de dados de um filho.</summary>
    [HttpPost("children/{studentId:guid}/consent")]
    [Authorize(Roles = nameof(PrimaryRole.Guardian))]
    [ProducesResponseType<GuardianConsentStatusResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GrantConsent(
        Guid studentId,
        GrantGuardianConsentRequest request,
        CancellationToken cancellationToken) =>
        FromResult(await consents.GrantAsync(TenantId, ProfileId, studentId, CurrentUserId, request, cancellationToken));

    /// <summary>Revoga o consentimento ativo de um filho (não apaga — o histórico permanece).</summary>
    [HttpPost("children/{studentId:guid}/consent/revoke")]
    [Authorize(Roles = nameof(PrimaryRole.Guardian))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RevokeConsent(Guid studentId, CancellationToken cancellationToken) =>
        NoContentOrProblem(await consents.RevokeAsync(TenantId, ProfileId, studentId, CurrentUserId, cancellationToken));
}
