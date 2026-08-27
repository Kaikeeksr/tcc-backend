using AttendanceManagement.Application.Attendance;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Abstractions;

/// <summary>Acesso à linha de aluno dentro de uma chamada. Escopado por tenant via a sessão/aluno relacionados.</summary>
public interface IAttendanceRecordRepository
{
    void Add(AttendanceRecord record);

    Task<AttendanceRecord?> GetForUpdateAsync(Guid transporterId, Guid sessionId, Guid studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceRecord>> GetForUpdateBySessionAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task<bool> StudentExistsAsync(Guid transporterId, Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>Verifica se o responsável pode retirar o aluno (vínculo ativo com <c>CanPickup</c>).</summary>
    Task<bool> CanGuardianPickupAsync(Guid guardianId, Guid studentId, CancellationToken cancellationToken = default);

    /// <summary>Relatório agregado por aluno de um grupo, num intervalo de datas (sessões fechadas).</summary>
    Task<TransportGroupAttendanceReport?> GetGroupReportAsync(
        Guid transporterId,
        Guid transportGroupId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    /// <summary>Histórico e agregado de um aluno específico, num intervalo de datas.</summary>
    Task<StudentAttendanceHistory?> GetStudentHistoryAsync(
        Guid transporterId,
        Guid studentId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);
}
