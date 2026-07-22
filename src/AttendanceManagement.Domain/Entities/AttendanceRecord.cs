using AttendanceManagement.Domain.Abstractions;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Domain.Entities;

/// <summary>
/// A linha de um aluno dentro de uma chamada. Uma por (sessão, aluno).
///
/// O caso "retirado pelo responsável" é tipado aqui, e não escondido em JSON,
/// para a tela do transportador ser uma consulta indexada simples.
/// </summary>
public sealed class AttendanceRecord : AuditableEntity
{
    public const int JustificationMaxLength = 500;

    private AttendanceRecord(
        Guid id,
        Guid attendanceSessionId,
        Guid studentId,
        Guid transporterId,
        AttendanceStatus status,
        Guid recordedBy,
        Guid? schoolId)
        : base(id)
    {
        AttendanceSessionId = attendanceSessionId;
        StudentId = studentId;
        TransporterId = transporterId;
        Status = status;
        RecordedBy = recordedBy;
        SchoolId = schoolId;
        RecordedAtUtc = DateTime.UtcNow;
    }

    private AttendanceRecord()
    {
    }

    public Guid AttendanceSessionId { get; private set; }

    public Guid StudentId { get; private set; }

    /// <summary>Denormalizado para filtro por tenant sem join na tabela de milhões.</summary>
    public Guid TransporterId { get; private set; }

    public AttendanceStatus Status { get; private set; }

    public Guid? PickedUpByGuardianId { get; private set; }

    public string? Justification { get; private set; }

    public Guid? JustifiedBy { get; private set; }

    /// <summary>Snapshot da escola do aluno no dia.</summary>
    public Guid? SchoolId { get; private set; }

    public DateTime RecordedAtUtc { get; private set; }

    public Guid RecordedBy { get; private set; }

    public static Result<AttendanceRecord> Mark(
        Guid attendanceSessionId,
        Guid studentId,
        Guid transporterId,
        AttendanceStatus status,
        Guid recordedBy,
        Guid? schoolId)
    {
        var guard = Validate(attendanceSessionId, studentId, transporterId, recordedBy);
        if (guard.IsFailure)
        {
            return Result.Failure<AttendanceRecord>(guard.Error);
        }

        return Result.Success(new AttendanceRecord(
            Guid.CreateVersion7(),
            attendanceSessionId,
            studentId,
            transporterId,
            status,
            recordedBy,
            schoolId == Guid.Empty ? null : schoolId));
    }

    /// <summary>
    /// Registra que o responsável retirou o aluno. A permissão de retirada
    /// (`GuardianStudent.CanPickup`) é validada na camada de aplicação.
    /// </summary>
    public static Result<AttendanceRecord> MarkPickedUpByGuardian(
        Guid attendanceSessionId,
        Guid studentId,
        Guid transporterId,
        Guid pickedUpByGuardianId,
        string? justification,
        Guid recordedBy,
        Guid? schoolId)
    {
        var guard = Validate(attendanceSessionId, studentId, transporterId, recordedBy);
        if (guard.IsFailure)
        {
            return Result.Failure<AttendanceRecord>(guard.Error);
        }

        if (pickedUpByGuardianId == Guid.Empty)
        {
            return Result.Failure<AttendanceRecord>(Error.Validation("AttendanceRecord.GuardianRequired", "O responsável que retirou é obrigatório."));
        }

        var record = new AttendanceRecord(
            Guid.CreateVersion7(),
            attendanceSessionId,
            studentId,
            transporterId,
            AttendanceStatus.PickedUpByGuardian,
            recordedBy,
            schoolId == Guid.Empty ? null : schoolId)
        {
            PickedUpByGuardianId = pickedUpByGuardianId,
            Justification = Trim(justification),
            JustifiedBy = pickedUpByGuardianId,
        };

        return Result.Success(record);
    }

    public void Justify(string? justification, Guid? justifiedBy)
    {
        Justification = Trim(justification);
        JustifiedBy = justifiedBy == Guid.Empty ? null : justifiedBy;

        if (Status == AttendanceStatus.Absent)
        {
            Status = AttendanceStatus.Justified;
        }

        Touch();
    }

    private static string? Trim(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Result Validate(Guid sessionId, Guid studentId, Guid transporterId, Guid recordedBy)
    {
        if (sessionId == Guid.Empty)
        {
            return Result.Failure(Error.Validation("AttendanceRecord.SessionRequired", "A sessão é obrigatória."));
        }

        if (studentId == Guid.Empty)
        {
            return Result.Failure(Error.Validation("AttendanceRecord.StudentRequired", "O aluno é obrigatório."));
        }

        if (transporterId == Guid.Empty)
        {
            return Result.Failure(Error.Validation("AttendanceRecord.TransporterRequired", "O transportador é obrigatório."));
        }

        if (recordedBy == Guid.Empty)
        {
            return Result.Failure(Error.Validation("AttendanceRecord.RecordedByRequired", "O autor do registro é obrigatório."));
        }

        return Result.Success();
    }
}
