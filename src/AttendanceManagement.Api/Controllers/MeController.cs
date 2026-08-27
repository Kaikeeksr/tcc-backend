using AttendanceManagement.Application.Attendance;
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
public sealed class MeController(GuardianStudentService guardianStudents, AttendanceService attendance) : ApiController
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
}
