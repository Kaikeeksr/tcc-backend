using AttendanceManagement.Application.Abstractions;

namespace AttendanceManagement.Application.Tests.Fakes;

/// <summary>Sem banco por trás: os fakes já aplicam a mudança na hora, então commitar é um no-op.</summary>
public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
}
