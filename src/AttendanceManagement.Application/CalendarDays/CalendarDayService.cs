using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.CalendarDays;

/// <summary>
/// Casos de uso do calendário letivo. Só os desvios (feriados) são gravados; uma
/// data sem linha é considerada letiva.
/// </summary>
public sealed class CalendarDayService(ICalendarDayRepository repository, IUnitOfWork unitOfWork)
{
    public Task<IReadOnlyList<CalendarDayResponse>> ListAsync(
        Guid transporterId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default) =>
        repository.ListAsync(transporterId, from, to, cancellationToken);

    public async Task<Result<CalendarDayResponse>> SetAsync(
        Guid transporterId,
        SetCalendarDayRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await repository.GetForUpdateAsync(transporterId, request.Date, cancellationToken);

        if (existing is not null)
        {
            existing.Update(request.Type, request.Description);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(new CalendarDayResponse(existing.Date, existing.Type, existing.Description));
        }

        var result = CalendarDay.Create(transporterId, request.Date, request.Type, request.Description);
        if (result.IsFailure)
        {
            return Result.Failure<CalendarDayResponse>(result.Error);
        }

        var day = result.Value;
        repository.Add(day);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CalendarDayResponse(day.Date, day.Type, day.Description));
    }

    public async Task<Result> RemoveAsync(
        Guid transporterId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetForUpdateAsync(transporterId, date, cancellationToken);
        if (existing is null)
        {
            return Result.Failure(Error.NotFound("CalendarDay.NotFound", "There is no override for this date."));
        }

        repository.Remove(existing);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
