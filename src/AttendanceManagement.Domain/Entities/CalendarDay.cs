using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// Marca uma data como letiva ou feriado para o calendário do transportador. Por
/// padrão (ausência de linha) uma data é letiva; só os desvios (feriados) são
/// gravados aqui.
/// </summary>
public sealed class CalendarDay : AuditableEntity
{
    public const int DescriptionMaxLength = 200;

    private CalendarDay(Guid id, Guid transporterId, DateOnly date, CalendarDayType type, string? description)
        : base(id)
    {
        TransporterId = transporterId;
        Date = date;
        Type = type;
        Description = description;
    }

    private CalendarDay()
    {
    }

    public Guid TransporterId { get; private set; }

    public DateOnly Date { get; private set; }

    public CalendarDayType Type { get; private set; }

    public string? Description { get; private set; }

    public Transporter Transporter { get; private set; } = null!;

    public static Result<CalendarDay> Create(
        Guid transporterId,
        DateOnly date,
        CalendarDayType type,
        string? description)
    {
        if (transporterId == Guid.Empty)
        {
            return Result.Failure<CalendarDay>(Error.Validation("CalendarDay.TransporterRequired", "Transporter is required."));
        }

        var normalizedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        if (normalizedDescription?.Length > DescriptionMaxLength)
        {
            return Result.Failure<CalendarDay>(Error.Validation("CalendarDay.DescriptionTooLong", "The description exceeds the maximum length."));
        }

        return Result.Success(new CalendarDay(Guid.CreateVersion7(), transporterId, date, type, normalizedDescription));
    }

    public void Update(CalendarDayType type, string? description)
    {
        Type = type;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Touch();
    }
}
