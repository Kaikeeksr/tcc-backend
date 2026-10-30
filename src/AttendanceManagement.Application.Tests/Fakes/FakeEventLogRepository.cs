using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Tests.Fakes;

public sealed class FakeEventLogRepository : IEventLogRepository
{
    public List<EventLog> Entries { get; } = [];

    public void Add(EventLog entry) => Entries.Add(entry);
}
