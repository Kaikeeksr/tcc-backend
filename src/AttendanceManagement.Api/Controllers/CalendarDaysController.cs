using AttendanceManagement.Application.CalendarDays;
using AttendanceManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Api.Controllers;

/// <summary>Calendário letivo do transportador. Só os desvios (feriados) são gravados.</summary>
[Route("api/calendar-days")]
[Authorize(Roles = nameof(PrimaryRole.Transporter))]
public sealed class CalendarDaysController(CalendarDayService service) : ApiController
{
    /// <summary>Lista os desvios (feriados) num intervalo de datas.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CalendarDayResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(TenantId, from, to, cancellationToken));

    /// <summary>Marca (ou atualiza) uma data como feriado/letiva.</summary>
    [HttpPut]
    [ProducesResponseType<CalendarDayResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Set(SetCalendarDayRequest request, CancellationToken cancellationToken) =>
        FromResult(await service.SetAsync(TenantId, request, cancellationToken));

    /// <summary>Remove o desvio de uma data (volta a ser letiva por padrão).</summary>
    [HttpDelete("{date}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(DateOnly date, CancellationToken cancellationToken) =>
        NoContentOrProblem(await service.RemoveAsync(TenantId, date, cancellationToken));
}
