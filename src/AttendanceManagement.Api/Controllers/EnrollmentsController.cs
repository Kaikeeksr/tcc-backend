using AttendanceManagement.Application.Enrollments;
using AttendanceManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Api.Controllers;

/// <summary>Matrículas: o vínculo (temporal) entre um aluno e um grupo de transporte.</summary>
[Authorize(Roles = nameof(PrimaryRole.Transporter))]
public sealed class EnrollmentsController(EnrollmentService service) : ApiController
{
    /// <summary>Lista as matrículas (ativas e encerradas) de um aluno.</summary>
    [HttpGet("api/students/{studentId:guid}/enrollments")]
    [ProducesResponseType<IReadOnlyList<EnrollmentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListByStudent(Guid studentId, CancellationToken cancellationToken) =>
        FromResult(await service.ListByStudentAsync(TenantId, studentId, cancellationToken));

    /// <summary>Matricula um aluno num grupo.</summary>
    [HttpPost("api/students/{studentId:guid}/enrollments")]
    [ProducesResponseType<EnrollmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enroll(Guid studentId, EnrollStudentRequest request, CancellationToken cancellationToken)
    {
        var result = await service.EnrollAsync(TenantId, studentId, request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ToProblem(result.Error);
    }

    /// <summary>Encerra uma matrícula (não apaga — o histórico permanece).</summary>
    [HttpDelete("api/enrollments/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> End(Guid id, CancellationToken cancellationToken) =>
        NoContentOrProblem(await service.EndAsync(TenantId, id, cancellationToken));
}
