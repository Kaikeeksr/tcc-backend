using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.Transporters;
using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Persistence.Repositories;

internal sealed class TransporterRepository(AppDbContext context) : ITransporterRepository
{
    public Task<Transporter?> GetByUserAccountAsync(
        Guid userAccountId,
        CancellationToken cancellationToken = default) =>
        context.Transporters
            .AsTracking()
            .FirstOrDefaultAsync(t => t.UserAccountId == userAccountId, cancellationToken);

    public Task<Transporter?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        var normalized = email.Trim().ToLowerInvariant();

        return context.Transporters
            .AsTracking()
            .FirstOrDefaultAsync(t => t.UserAccount.Email == normalized, cancellationToken);
    }

    public void Add(Transporter transporter)
    {
        ArgumentNullException.ThrowIfNull(transporter);

        context.Transporters.Add(transporter);
    }

    public Task<TransportTeam?> GetTeamByUserAccountAsync(
        Guid userAccountId,
        CancellationToken cancellationToken = default) =>
        context.Transporters
            .Where(t => t.UserAccountId == userAccountId)
            .Select(t => new TransportTeam(
                t.Name,
                context.TransportGroups
                    .Where(g => g.TransporterId == t.Id)
                    .OrderBy(g => g.Name)
                    .Select(g => new TransportGroupSummary(
                        g.Id,
                        g.Name,
                        g.Shift,
                        g.Assistant == null ? null : new AssistantSummary(g.Assistant.Id, g.Assistant.Name),
                        g.Vehicle == null ? null : new VehicleSummary(g.Vehicle.Id, g.Vehicle.Plate, g.Vehicle.Model),
                        // Parte de Students (não de Enrollments): enrollment não
                        // filtra soft delete, então navegar por ele traria aluno
                        // fantasma. O filtro global do aluno resolve isso.
                        context.Students
                            .Where(s => context.Enrollments.Any(e =>
                                e.StudentId == s.Id && e.TransportGroupId == g.Id && e.Active))
                            .OrderBy(s => s.Name)
                            .Select(s => new StudentSummary(
                                s.Id,
                                s.Name,
                                s.Grade,
                                s.School == null ? null : new SchoolSummary(s.School.Id, s.School.Name),
                                // Join a partir de Guardian pelo mesmo motivo: o
                                // inner join só casa quem existe dos dois lados.
                                (from gd in context.Guardians
                                 join gs in context.GuardianStudents on gd.Id equals gs.GuardianId
                                 where gs.StudentId == s.Id && gs.Active
                                 orderby gs.IsPrimary descending, gd.Name
                                 select new GuardianSummary(
                                     gd.Id,
                                     gd.Name,
                                     gs.Relationship,
                                     gs.IsPrimary,
                                     gs.CanPickup,
                                     gd.Mobile,
                                     gd.Phone,
                                     gd.Whatsapp))
                                .ToList()))
                            .ToList()))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
}
