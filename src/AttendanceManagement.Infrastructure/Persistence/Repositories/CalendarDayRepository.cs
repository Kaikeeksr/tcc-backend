using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.CalendarDays;
using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Persistence.Repositories;

internal sealed class CalendarDayRepository(AppDbContext context) : ICalendarDayRepository
{
    public void Add(CalendarDay day)
    {
        ArgumentNullException.ThrowIfNull(day);

        context.CalendarDays.Add(day);
    }

    public void Remove(CalendarDay day)
    {
        ArgumentNullException.ThrowIfNull(day);

        context.CalendarDays.Remove(day);
    }

    public Task<CalendarDay?> GetForUpdateAsync(Guid transporterId, DateOnly date, CancellationToken cancellationToken = default) =>
        context.CalendarDays
            .AsTracking()
            .FirstOrDefaultAsync(d => d.TransporterId == transporterId && d.Date == date, cancellationToken);

    public async Task<IReadOnlyList<CalendarDayResponse>> ListAsync(
        Guid transporterId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default) =>
        await context.CalendarDays
            .Where(d => d.TransporterId == transporterId && d.Date >= from && d.Date <= to)
            .OrderBy(d => d.Date)
            .Select(d => new CalendarDayResponse(d.Date, d.Type, d.Description))
            .ToListAsync(cancellationToken);
}
