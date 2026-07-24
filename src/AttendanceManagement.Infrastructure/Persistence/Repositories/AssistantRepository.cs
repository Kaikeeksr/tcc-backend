using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.Assistants;
using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Persistence.Repositories;

internal sealed class AssistantRepository(AppDbContext context) : IAssistantRepository
{
    public void Add(Assistant assistant)
    {
        ArgumentNullException.ThrowIfNull(assistant);

        context.Assistants.Add(assistant);
    }

    public Task<Assistant?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        context.Assistants
            .AsTracking()
            .Include(a => a.UserAccount)
            .FirstOrDefaultAsync(a => a.Id == id && a.TransporterId == transporterId, cancellationToken);

    public Task<AssistantResponse?> GetByIdAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default) =>
        context.Assistants
            .Where(a => a.Id == id && a.TransporterId == transporterId)
            .Select(a => new AssistantResponse(a.Id, a.Name, a.UserAccount.Email, a.UserAccountId))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<AssistantResponse>> ListAsync(Guid transporterId, CancellationToken cancellationToken = default) =>
        await context.Assistants
            .Where(a => a.TransporterId == transporterId)
            .OrderBy(a => a.Name)
            .Select(a => new AssistantResponse(a.Id, a.Name, a.UserAccount.Email, a.UserAccountId))
            .ToListAsync(cancellationToken);

    public Task<bool> IsAssignedToGroupAsync(Guid assistantId, CancellationToken cancellationToken = default) =>
        context.TransportGroups.AnyAsync(g => g.AssistantId == assistantId, cancellationToken);
}
