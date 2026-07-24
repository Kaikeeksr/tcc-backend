using AttendanceManagement.Application.Authentication;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Domain.Enums;

namespace AttendanceManagement.Application.Abstractions;

/// <summary>Acesso à conta de login e à identidade derivada do perfil.</summary>
public interface IUserAccountRepository
{
    Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolve nome e tenant na tabela do perfil que o <paramref name="primaryRole"/> indica.
    /// <c>null</c> quando a conta não tem perfil vinculado.
    /// </summary>
    Task<UserIdentity?> GetIdentityAsync(
        Guid userAccountId,
        PrimaryRole primaryRole,
        CancellationToken cancellationToken = default);

    void Add(UserAccount account);
}
