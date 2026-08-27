using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Abstractions;

/// <summary>Escrita na trilha de auditoria append-only. Não há leitura pela aplicação por ora.</summary>
public interface IEventLogRepository
{
    void Add(EventLog entry);
}
