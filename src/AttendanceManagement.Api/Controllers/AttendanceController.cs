using AttendanceManagement.Application.Attendance;
using AttendanceManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Api.Controllers;

/// <summary>
/// A chamada: abrir a sessão do dia (roster automático dos matriculados ativos),
/// marcar presença em lote, registrar retirada pelo responsável, justificar,
/// fechar/cancelar, e os relatórios agregados por grupo/aluno.
/// </summary>
[Authorize(Roles = nameof(PrimaryRole.Transporter))]
public sealed class AttendanceController(AttendanceService service) : ApiController
{
    /// <summary>Abre a chamada do dia para um grupo (ida ou volta). O roster nasce dos matriculados ativos.</summary>
    [HttpPost("api/transport-groups/{groupId:guid}/attendance-sessions")]
    [ProducesResponseType<AttendanceSessionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Open(Guid groupId, OpenAttendanceSessionRequest request, CancellationToken cancellationToken)
    {
        var result = await service.OpenAsync(TenantId, groupId, CurrentUserId, request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ToProblem(result.Error);
    }

    /// <summary>Busca a sessão de um grupo numa data/sentido específicos, se existir.</summary>
    [HttpGet("api/transport-groups/{groupId:guid}/attendance-sessions/by-date")]
    [ProducesResponseType<AttendanceSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByDate(
        Guid groupId,
        [FromQuery] DateOnly date,
        [FromQuery] SessionType type,
        CancellationToken cancellationToken) =>
        FromResult(await service.GetByGroupAndDateAsync(TenantId, groupId, date, type, cancellationToken));

    /// <summary>Lista o histórico (cabeçalhos) das sessões de um grupo num intervalo de datas.</summary>
    [HttpGet("api/transport-groups/{groupId:guid}/attendance-sessions")]
    [ProducesResponseType<IReadOnlyList<AttendanceSessionSummary>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListByGroup(
        Guid groupId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken) =>
        Ok(await service.ListByGroupAsync(TenantId, groupId, from, to, cancellationToken));

    /// <summary>Obtém a sessão (cabeçalho + roster) pelo id.</summary>
    [HttpGet("api/attendance-sessions/{id:guid}")]
    [ProducesResponseType<AttendanceSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        FromResult(await service.GetAsync(TenantId, id, cancellationToken));

    /// <summary>Marca presença/falta/atraso/justificado em lote. Retirada pelo responsável tem endpoint próprio.</summary>
    [HttpPut("api/attendance-sessions/{id:guid}/records")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MarkRecords(Guid id, MarkAttendanceRecordsRequest request, CancellationToken cancellationToken) =>
        NoContentOrProblem(await service.MarkRecordsAsync(TenantId, id, CurrentUserId, request, cancellationToken));

    /// <summary>Registra que o responsável retirou o aluno direto na escola — não é falta.</summary>
    [HttpPost("api/attendance-sessions/{id:guid}/records/{studentId:guid}/pickup")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Pickup(
        Guid id,
        Guid studentId,
        MarkPickedUpByGuardianRequest request,
        CancellationToken cancellationToken) =>
        NoContentOrProblem(await service.MarkPickedUpByGuardianAsync(TenantId, id, studentId, CurrentUserId, request, cancellationToken));

    /// <summary>Justifica uma falta (ou qualquer registro).</summary>
    [HttpPut("api/attendance-sessions/{id:guid}/records/{studentId:guid}/justify")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Justify(
        Guid id,
        Guid studentId,
        JustifyAttendanceRecordRequest request,
        CancellationToken cancellationToken) =>
        NoContentOrProblem(await service.JustifyAsync(TenantId, id, studentId, request, cancellationToken));

    /// <summary>Fecha a chamada — não admite mais edição de registros.</summary>
    [HttpPost("api/attendance-sessions/{id:guid}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken) =>
        NoContentOrProblem(await service.CloseAsync(TenantId, id, CurrentUserId, cancellationToken));

    /// <summary>Cancela a chamada (ex.: aberta por engano).</summary>
    [HttpPost("api/attendance-sessions/{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken) =>
        NoContentOrProblem(await service.CancelAsync(TenantId, id, CurrentUserId, cancellationToken));

    /// <summary>Relatório de frequência do grupo, agregado por aluno, num intervalo de datas.</summary>
    [HttpGet("api/transport-groups/{groupId:guid}/attendance-report")]
    [ProducesResponseType<TransportGroupAttendanceReport>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGroupReport(
        Guid groupId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken) =>
        FromResult(await service.GetGroupReportAsync(TenantId, groupId, from, to, cancellationToken));

    /// <summary>Histórico e agregado de frequência de um aluno específico.</summary>
    [HttpGet("api/students/{studentId:guid}/attendance-history")]
    [ProducesResponseType<StudentAttendanceHistory>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentHistory(
        Guid studentId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken) =>
        FromResult(await service.GetStudentHistoryAsync(TenantId, studentId, from, to, cancellationToken));
}
