namespace AttendanceManagement.Application.Authentication;

/// <summary>
/// Nomes das claims do JWT — fonte única, usada por quem assina (Infrastructure)
/// e por quem lê (a Api, no /me e na configuração do JwtBearer).
/// </summary>
public static class AuthClaims
{
    public const string Subject = "sub";
    public const string Email = "email";
    public const string Name = "name";

    /// <summary>Papel primário. Mapeado como role do ASP.NET para habilitar <c>[Authorize(Roles = ...)]</c>.</summary>
    public const string Role = "role";

    public const string TransporterId = "transporter_id";
    public const string ProfileId = "profile_id";
}
