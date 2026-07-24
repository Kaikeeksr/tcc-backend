namespace AttendanceManagement.Domain.Enums;

public enum AttendanceStatus
{
    Present = 0,
    Absent = 1,
    Late = 2,

    /// <summary>Retirado direto pelo responsável. Não é ausência: carrega responsável, justificativa e hora.</summary>
    PickedUpByGuardian = 3,

    Justified = 4,
}
