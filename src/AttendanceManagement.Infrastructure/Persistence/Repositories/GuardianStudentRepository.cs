using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.GuardianStudents;
using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Persistence.Repositories;

internal sealed class GuardianStudentRepository(AppDbContext context) : IGuardianStudentRepository
{
    public void Add(GuardianStudent link)
    {
        ArgumentNullException.ThrowIfNull(link);

        context.GuardianStudents.Add(link);
    }

    public Task<GuardianStudent?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        context.GuardianStudents
            .AsTracking()
            .Where(gs => gs.Id == id)
            .Where(gs => context.Students.Any(s => s.Id == gs.StudentId && s.TransporterId == transporterId))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<GuardianStudent?> GetActivePrimaryAsync(Guid studentId, Guid? excludeId, CancellationToken cancellationToken = default) =>
        context.GuardianStudents
            .AsTracking()
            .Where(gs => gs.StudentId == studentId && gs.IsPrimary && gs.Active)
            .Where(gs => excludeId == null || gs.Id != excludeId)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<bool> StudentExistsAsync(Guid transporterId, Guid studentId, CancellationToken cancellationToken = default) =>
        context.Students.AnyAsync(s => s.Id == studentId && s.TransporterId == transporterId, cancellationToken);

    public Task<bool> GuardianExistsAsync(Guid transporterId, Guid guardianId, CancellationToken cancellationToken = default) =>
        context.Guardians.AnyAsync(g => g.Id == guardianId && g.TransporterId == transporterId, cancellationToken);

    public Task<bool> ActivePairExistsAsync(Guid guardianId, Guid studentId, CancellationToken cancellationToken = default) =>
        context.GuardianStudents.AnyAsync(
            gs => gs.GuardianId == guardianId && gs.StudentId == studentId && gs.Active,
            cancellationToken);

    public async Task<IReadOnlyList<GuardianStudentResponse>> ListByStudentAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        await context.GuardianStudents
            .Where(gs => gs.StudentId == studentId)
            .OrderByDescending(gs => gs.Active)
            .ThenByDescending(gs => gs.IsPrimary)
            .Select(gs => new GuardianStudentResponse(
                gs.Id,
                gs.GuardianId,
                gs.StudentId,
                gs.Relationship,
                gs.IsPrimary,
                gs.CanPickup,
                gs.Active))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<MyChildResponse>> ListByGuardianAsync(Guid guardianId, CancellationToken cancellationToken = default) =>
        await context.GuardianStudents
            .Where(gs => gs.GuardianId == guardianId && gs.Active)
            .OrderByDescending(gs => gs.IsPrimary)
            .ThenBy(gs => gs.Student.Name)
            .Select(gs => new MyChildResponse(
                gs.StudentId,
                gs.Student.Name,
                gs.Student.Grade,
                gs.Relationship,
                gs.IsPrimary,
                gs.CanPickup))
            .ToListAsync(cancellationToken);
}
