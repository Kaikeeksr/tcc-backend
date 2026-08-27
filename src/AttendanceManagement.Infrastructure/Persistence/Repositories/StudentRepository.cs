using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.Students;
using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Persistence.Repositories;

internal sealed class StudentRepository(AppDbContext context) : IStudentRepository
{
    public void Add(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);

        context.Students.Add(student);
    }

    public Task<Student?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        context.Students
            .AsTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.TransporterId == transporterId, cancellationToken);

    public Task<StudentResponse?> GetByIdAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        context.Students
            .Where(s => s.Id == id && s.TransporterId == transporterId)
            .Select(s => new StudentResponse(s.Id, s.Name, s.BirthDate, s.Grade, s.SchoolId, s.UserAccountId != null))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<StudentResponse>> ListAsync(Guid transporterId, CancellationToken cancellationToken = default) =>
        await context.Students
            .Where(s => s.TransporterId == transporterId)
            .OrderBy(s => s.Name)
            .Select(s => new StudentResponse(s.Id, s.Name, s.BirthDate, s.Grade, s.SchoolId, s.UserAccountId != null))
            .ToListAsync(cancellationToken);

    public Task<bool> SchoolExistsAsync(Guid transporterId, Guid schoolId, CancellationToken cancellationToken = default) =>
        context.Schools.AnyAsync(s => s.Id == schoolId && s.TransporterId == transporterId, cancellationToken);

    public async Task<bool> HasActiveLinksAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        await context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.Active, cancellationToken) ||
        await context.GuardianStudents.AnyAsync(gs => gs.StudentId == studentId && gs.Active, cancellationToken);
}
