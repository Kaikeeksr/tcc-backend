using System.Linq.Expressions;
using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.Common;
using AttendanceManagement.Application.Guardians;
using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Persistence.Repositories;

internal sealed class GuardianRepository(AppDbContext context) : IGuardianRepository
{
    public void Add(Guardian guardian)
    {
        ArgumentNullException.ThrowIfNull(guardian);

        context.Guardians.Add(guardian);
    }

    public Task<Guardian?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        context.Guardians
            .AsTracking()
            .Include(g => g.UserAccount)
            .FirstOrDefaultAsync(g => g.Id == id && g.TransporterId == transporterId, cancellationToken);

    public Task<GuardianResponse?> GetByIdAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        context.Guardians
            .Where(g => g.Id == id && g.TransporterId == transporterId)
            .Select(Projection)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<GuardianResponse>> ListAsync(Guid transporterId, CancellationToken cancellationToken = default) =>
        await context.Guardians
            .Where(g => g.TransporterId == transporterId)
            .OrderBy(g => g.Name)
            .Select(Projection)
            .ToListAsync(cancellationToken);

    public Task<bool> HasActiveStudentsAsync(Guid guardianId, CancellationToken cancellationToken = default) =>
        context.GuardianStudents.AnyAsync(gs => gs.GuardianId == guardianId && gs.Active, cancellationToken);

    private static readonly Expression<Func<Guardian, GuardianResponse>> Projection =
        g => new GuardianResponse(
            g.Id,
            g.Name,
            g.UserAccount.Email,
            g.UserAccountId,
            new GuardianContactPayload(g.Phone, g.Mobile, g.Whatsapp, g.ContactEmail),
            new AddressPayload(
                g.Address.Street,
                g.Address.Number,
                g.Address.Complement,
                g.Address.District,
                g.Address.City,
                g.Address.State,
                g.Address.PostalCode,
                g.Address.Latitude,
                g.Address.Longitude));
}
