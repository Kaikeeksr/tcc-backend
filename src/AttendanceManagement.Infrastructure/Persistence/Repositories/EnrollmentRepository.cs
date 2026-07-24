using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.Enrollments;
using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Persistence.Repositories;

internal sealed class EnrollmentRepository(AppDbContext context) : IEnrollmentRepository
{
    public void Add(Enrollment enrollment)
    {
        ArgumentNullException.ThrowIfNull(enrollment);

        context.Enrollments.Add(enrollment);
    }

    public Task<Enrollment?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        context.Enrollments
            .AsTracking()
            .Where(e => e.Id == id)
            .Where(e => context.Students.Any(s => s.Id == e.StudentId && s.TransporterId == transporterId))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> StudentExistsAsync(Guid transporterId, Guid studentId, CancellationToken cancellationToken = default) =>
        context.Students.AnyAsync(s => s.Id == studentId && s.TransporterId == transporterId, cancellationToken);

    public Task<string?> GetGroupNameAsync(Guid transporterId, Guid groupId, CancellationToken cancellationToken = default) =>
        context.TransportGroups
            .Where(g => g.Id == groupId && g.TransporterId == transporterId)
            .Select(g => g.Name)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> ActiveExistsAsync(Guid studentId, Guid groupId, CancellationToken cancellationToken = default) =>
        context.Enrollments.AnyAsync(
            e => e.StudentId == studentId && e.TransportGroupId == groupId && e.Active,
            cancellationToken);

    public async Task<IReadOnlyList<EnrollmentResponse>> ListByStudentAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        await context.Enrollments
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.StartedAtUtc)
            .Select(e => new EnrollmentResponse(
                e.Id,
                e.StudentId,
                e.TransportGroupId,
                e.TransportGroup.Name,
                e.Active,
                e.StartedAtUtc,
                e.EndedAtUtc))
            .ToListAsync(cancellationToken);
}
