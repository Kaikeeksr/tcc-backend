using AttendanceManagement.Application.Assistants;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Abstractions;

/// <summary>Acesso ao monitor do transportador. A escrita rastreia o monitor com a conta de login.</summary>
public interface IAssistantRepository
{
    void Add(Assistant assistant);

    /// <summary>Monitor rastreado com a <c>UserAccount</c> carregada, para editar/remover e poder bloquear o login.</summary>
    Task<Assistant?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default);

    Task<AssistantResponse?> GetByIdAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssistantResponse>> ListAsync(Guid transporterId, CancellationToken cancellationToken = default);

    /// <summary>O monitor está designado a algum grupo? Bloqueia a remoção.</summary>
    Task<bool> IsAssignedToGroupAsync(Guid assistantId, CancellationToken cancellationToken = default);
}
