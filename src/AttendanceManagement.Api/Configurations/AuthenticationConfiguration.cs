using System.Text;
using AttendanceManagement.Application.Authentication;
using AttendanceManagement.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AttendanceManagement.Api.Configurations;

/// <summary>Validação do Bearer token no pipeline. A assinatura acontece na Infrastructure.</summary>
internal static class AuthenticationConfiguration
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Mesma seção e mesma classe que a Infrastructure usa para assinar — emissor,
        // audiência e chave precisam bater dos dois lados.
        var options = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        options.Validate();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                // Sem o mapeamento legado de claims: "sub"/"role"/"email" chegam ao
                // ClaimsPrincipal com o mesmo nome que saiu no token.
                jwt.MapInboundClaims = false;

                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,
                    ValidateAudience = true,
                    ValidAudience = options.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    RoleClaimType = AuthClaims.Role,
                    NameClaimType = AuthClaims.Name,
                };
            });

        services.AddAuthorization();

        return services;
    }
}
