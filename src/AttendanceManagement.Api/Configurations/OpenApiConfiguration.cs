// Na v2 do Microsoft.OpenApi o namespace .Models foi achatado: OpenApiInfo mora
// direto em Microsoft.OpenApi.
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace AttendanceManagement.Api.Configurations;

/// <summary>
/// Documentação da API. No .NET 10 o documento OpenAPI 3.1 é gerado nativamente
/// pelo Microsoft.AspNetCore.OpenApi; o Scalar entra só como interface. Os
/// comentários XML dos controllers entram no documento automaticamente.
/// </summary>
internal static class OpenApiConfiguration
{
    private const string BearerScheme = "Bearer";

    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "Attendance Management API",
                    Version = "v1",
                    Description = "TCC API for school transport attendance management.",
                };

                // Declara o esquema Bearer/JWT no documento — sem isso o botão
                // Authorize do Scalar não teria o que preencher e o token nunca
                // seria enviado nos endpoints protegidos.
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[BearerScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Cole o token devolvido por /api/auth/login (sem o prefixo 'Bearer').",
                };

                // Requisito global: o cadeado aparece em todos os endpoints. Os
                // marcados com [AllowAnonymous] continuam abertos mesmo assim.
                document.Security =
                [
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(BearerScheme, document)] = [],
                    },
                ];

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static WebApplication UseOpenApiConfiguration(this WebApplication app)
    {
        app.MapOpenApi();

        app.MapScalarApiReference(options =>
        {
            options.Title = "Attendance Management API";
            options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        return app;
    }
}
