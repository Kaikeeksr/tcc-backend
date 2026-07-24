using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.Common;
using AttendanceManagement.Application.Schools;
using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Persistence.Repositories;

internal sealed class SchoolRepository(AppDbContext context) : ISchoolRepository
{
    public void Add(School school)
    {
        ArgumentNullException.ThrowIfNull(school);

        context.Schools.Add(school);
    }

    public Task<School?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        context.Schools
            .AsTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.TransporterId == transporterId, cancellationToken);

    public Task<SchoolResponse?> GetByIdAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        context.Schools
            .Where(s => s.Id == id && s.TransporterId == transporterId)
            .Select(Projection)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<SchoolResponse>> ListAsync(Guid transporterId, CancellationToken cancellationToken = default) =>
        await context.Schools
            .Where(s => s.TransporterId == transporterId)
            .OrderBy(s => s.Name)
            .Select(Projection)
            .ToListAsync(cancellationToken);

    public Task<bool> NameExistsAsync(Guid transporterId, string name, Guid? excludeId, CancellationToken cancellationToken = default) =>
        context.Schools
            .Where(s => s.TransporterId == transporterId && s.Name == name)
            .Where(s => excludeId == null || s.Id != excludeId)
            .AnyAsync(cancellationToken);

    private static readonly System.Linq.Expressions.Expression<Func<School, SchoolResponse>> Projection =
        s => new SchoolResponse(
            s.Id,
            s.Name,
            new AddressPayload(
                s.Address.Street,
                s.Address.Number,
                s.Address.Complement,
                s.Address.District,
                s.Address.City,
                s.Address.State,
                s.Address.PostalCode,
                s.Address.Latitude,
                s.Address.Longitude));
}
