using AttendanceManagement.Application.Authentication;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Abstractions;

/// <summary>Emite o JWT assinado a partir da conta e do perfil. A validação acontece no pipeline da Api.</summary>
public interface IJwtTokenGenerator
{
    AccessToken Generate(UserAccount account, UserIdentity identity);
}
