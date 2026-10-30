using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.GuardianStudents;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Tests.Fakes;

public sealed class FakeGuardianStudentRepository : IGuardianStudentRepository
{
    /// <summary>Pares (responsável, aluno) com vínculo ativo.</summary>
    public HashSet<(Guid GuardianId, Guid StudentId)> ActivePairs { get; } = [];

    public void Add(GuardianStudent link) => throw new NotSupportedException("Not exercised by these tests.");

    public Task<GuardianStudent?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not exercised by these tests.");

    public Task<GuardianStudent?> GetActivePrimaryAsync(Guid studentId, Guid? excludeId, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not exercised by these tests.");

    public Task<bool> StudentExistsAsync(Guid transporterId, Guid studentId, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not exercised by these tests.");

    public Task<bool> GuardianExistsAsync(Guid transporterId, Guid guardianId, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not exercised by these tests.");

    public Task<bool> ActivePairExistsAsync(Guid guardianId, Guid studentId, CancellationToken cancellationToken = default) =>
        Task.FromResult(ActivePairs.Contains((guardianId, studentId)));

    public Task<IReadOnlyList<GuardianStudentResponse>> ListByStudentAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not exercised by these tests.");

    public Task<IReadOnlyList<MyChildResponse>> ListByGuardianAsync(Guid guardianId, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not exercised by these tests.");
}
