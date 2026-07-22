namespace AttendanceManagement.Domain.Enums;

/// <summary>
/// Atalho para rotear a tela inicial do app. O papel real deriva de qual perfil
/// referencia a conta.
/// </summary>
public enum PrimaryRole
{
    Transporter = 0,
    Assistant = 1,
    Guardian = 2,
    Student = 3,
}
