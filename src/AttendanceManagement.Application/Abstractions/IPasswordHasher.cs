namespace AttendanceManagement.Application.Abstractions;

/// <summary>Deriva e confere o hash da senha. O algoritmo vive na Infrastructure.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    /// <summary>Confere em tempo constante. Devolve <c>false</c> — nunca lança — se o hash estiver malformado.</summary>
    bool Verify(string password, string passwordHash);
}
