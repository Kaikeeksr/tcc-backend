using AttendanceManagement.Application.Attendance;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Application.Abstractions;

/// <summary>Acesso à sessão de chamada (cabeçalho). Escopado por tenant.</summary>
public interface IAttendanceSessionRepository
{
    void Add(AttendanceSession session);

    Task<AttendanceSession?> GetForUpdateAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid transportGroupId, DateOnly sessionDate, SessionType sessionType, CancellationToken cancellationToken = default);

    /// <summary>Nome do grupo e sua equipe designada, se o grupo pertence ao tenant.</summary>
    Task<(string Name, Guid? VehicleId, Guid? AssistantId)?> GetGroupInfoAsync(Guid transporterId, Guid groupId, CancellationToken cancellationToken = default);

    /// <summary>Alunos com matrícula ativa no grupo, para montar o roster ao abrir a sessão.</summary>
    Task<IReadOnlyList<RosterEntry>> GetActiveRosterAsync(Guid transportGroupId, CancellationToken cancellationToken = default);

    Task<AttendanceSessionResponse?> GetDetailAsync(Guid transporterId, Guid id, CancellationToken cancellationToken = default);

    Task<AttendanceSessionResponse?> GetDetailByGroupAndDateAsync(
        Guid transporterId,
        Guid transportGroupId,
        DateOnly sessionDate,
        SessionType sessionType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceSessionSummary>> ListByGroupAsync(
        Guid transporterId,
        Guid transportGroupId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);
}

public sealed record RosterEntry(Guid StudentId, string StudentName, Guid? SchoolId);
