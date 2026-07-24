using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.Authentication;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Persistence.Repositories;

internal sealed class UserAccountRepository(AppDbContext context) : IUserAccountRepository
{
    public Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        var normalized = email.Trim().ToLowerInvariant();

        return context.UserAccounts
            .FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        var normalized = email.Trim().ToLowerInvariant();

        return context.UserAccounts
            .AnyAsync(u => u.Email == normalized, cancellationToken);
    }

    public Task<UserIdentity?> GetIdentityAsync(
        Guid userAccountId,
        PrimaryRole primaryRole,
        CancellationToken cancellationToken = default)
    {
        // Os filtros globais de soft delete garantem que um perfil apagado não
        // completa login. Para o transportador, id do perfil e do tenant coincidem.
        return primaryRole switch
        {
            PrimaryRole.Transporter => context.Transporters
                .Where(t => t.UserAccountId == userAccountId)
                .Select(t => new UserIdentity(t.Id, t.Name, t.Id))
                .FirstOrDefaultAsync(cancellationToken),

            PrimaryRole.Assistant => context.Assistants
                .Where(a => a.UserAccountId == userAccountId)
                .Select(a => new UserIdentity(a.Id, a.Name, a.TransporterId))
                .FirstOrDefaultAsync(cancellationToken),

            PrimaryRole.Guardian => context.Guardians
                .Where(g => g.UserAccountId == userAccountId)
                .Select(g => new UserIdentity(g.Id, g.Name, g.TransporterId))
                .FirstOrDefaultAsync(cancellationToken),

            PrimaryRole.Student => context.Students
                .Where(s => s.UserAccountId == userAccountId)
                .Select(s => new UserIdentity(s.Id, s.Name, s.TransporterId))
                .FirstOrDefaultAsync(cancellationToken),

            _ => Task.FromResult<UserIdentity?>(null),
        };
    }

    public void Add(UserAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);

        context.UserAccounts.Add(account);
    }
}
