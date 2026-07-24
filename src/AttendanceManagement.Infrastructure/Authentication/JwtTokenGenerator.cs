using System.Text;
using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Application.Authentication;
using AttendanceManagement.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AttendanceManagement.Infrastructure.Authentication;

internal sealed class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
    private readonly JwtOptions _options = options.Value;

    public AccessToken Generate(UserAccount account, UserIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(identity);

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Expires = expiresAtUtc,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),

            // Dicionário cru (não ClaimsIdentity): os nomes saem no token como
            // escritos, sem o mapeamento de tipos legado trocar "sub"/"role" por URIs.
            Claims = new Dictionary<string, object>
            {
                [AuthClaims.Subject] = account.Id.ToString(),
                [AuthClaims.Email] = account.Email,
                [AuthClaims.Name] = identity.DisplayName,
                [AuthClaims.Role] = account.PrimaryRole.ToString(),
                [AuthClaims.TransporterId] = identity.TransporterId.ToString(),
                [AuthClaims.ProfileId] = identity.ProfileId.ToString(),
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
            },
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return new AccessToken(token, expiresAtUtc);
    }
}
