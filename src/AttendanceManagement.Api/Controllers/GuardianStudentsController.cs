using AttendanceManagement.Application.GuardianStudents;
using AttendanceManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Api.Controllers;

/// <summary>Vínculos (temporais) entre responsáveis e alunos.</summary>
[Authorize(Roles = nameof(PrimaryRole.Transporter))]
public sealed class GuardianStudentsController(GuardianStudentService service) : ApiController
{
    /// <summary>Lista os responsáveis vinculados (ativos e encerrados) de um aluno.</summary>
    [HttpGet("api/students/{studentId:guid}/guardians")]
    [ProducesResponseType<IReadOnlyList<GuardianStudentResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListByStudent(Guid studentId, CancellationToken cancellationToken) =>
        FromResult(await service.ListByStudentAsync(TenantId, studentId, cancellationToken));

    /// <summary>Vincula um responsável a um aluno.</summary>
    [HttpPost("api/students/{studentId:guid}/guardians")]
    [ProducesResponseType<GuardianStudentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Link(Guid studentId, LinkGuardianRequest request, CancellationToken cancellationToken)
    {
        var result = await service.LinkAsync(TenantId, studentId, request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ToProblem(result.Error);
    }

    /// <summary>Atualiza a relação e os flags (principal, pode retirar) de um vínculo.</summary>
    [HttpPut("api/guardian-students/{id:guid}")]
    [ProducesResponseType<GuardianStudentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, UpdateGuardianStudentRequest request, CancellationToken cancellationToken) =>
        FromResult(await service.UpdateAsync(TenantId, id, request, cancellationToken));

    /// <summary>Encerra um vínculo (não apaga — o histórico permanece).</summary>
    [HttpDelete("api/guardian-students/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> End(Guid id, CancellationToken cancellationToken) =>
        NoContentOrProblem(await service.EndAsync(TenantId, id, cancellationToken));
}
