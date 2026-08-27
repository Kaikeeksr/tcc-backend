using AttendanceManagement.Domain.Entities;
using FluentValidation;

namespace AttendanceManagement.Application.CalendarDays;

public sealed class SetCalendarDayRequestValidator : AbstractValidator<SetCalendarDayRequest>
{
    public SetCalendarDayRequestValidator()
    {
        RuleFor(x => x.Date).NotEqual(default(DateOnly));
        RuleFor(x => x.Description).MaximumLength(CalendarDay.DescriptionMaxLength);
    }
}
