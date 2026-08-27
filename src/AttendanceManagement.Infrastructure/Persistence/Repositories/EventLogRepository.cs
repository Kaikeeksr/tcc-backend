using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Infrastructure.Persistence.Repositories;

internal sealed class EventLogRepository(AppDbContext context) : IEventLogRepository
{
    public void Add(EventLog entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        context.EventLogs.Add(entry);
    }
}
