using AttendanceManagement.Application.CalendarDays;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Abstractions;

/// <summary>Acesso ao calendário letivo do transportador. Só os desvios (feriados) são gravados.</summary>
public interface ICalendarDayRepository
{
    void Add(CalendarDay day);

    void Remove(CalendarDay day);

    Task<CalendarDay?> GetForUpdateAsync(Guid transporterId, DateOnly date, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CalendarDayResponse>> ListAsync(
        Guid transporterId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);
}
