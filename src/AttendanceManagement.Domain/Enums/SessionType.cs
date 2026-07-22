namespace AttendanceManagement.Domain.Enums;

public enum SessionType
{
    /// <summary>Casa → escola.</summary>
    ToSchool = 0,

    /// <summary>Escola → casa. É onde ocorre o "retirado pelo responsável".</summary>
    FromSchool = 1,
}
