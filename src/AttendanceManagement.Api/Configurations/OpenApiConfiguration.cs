// Na v2 do Microsoft.OpenApi o namespace .Models foi achatado: OpenApiInfo
// mora direto em Microsoft.OpenApi. Codigo de tutorial antigo ainda usa .Models.
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace AttendanceManagement.Api.Configurations;

/// <summary>
/// Documentacao da API.
///
/// No .NET 10 a Microsoft tirou o Swashbuckle do template: o documento OpenAPI
/// (versao 3.1) passou a ser gerado nativamente pelo Microsoft.AspNetCore.OpenApi.
/// O Scalar entra so como interface — ele consome o documento, nao o gera.
///
/// Os comentarios XML dos controllers entram no documento automaticamente,
/// porque GenerateDocumentationFile esta ligado no Directory.Build.props.
/// </summary>
internal static class OpenApiConfiguration
{
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "API de Gestão de Chamadas",
                    Version = "v1",
                    Description =
                        "API do TCC. No momento expõe apenas o slice de exemplo `Alumni`, " +
                        "usado para validar a arquitetura em camadas ponta a ponta.",
                };

                return Task.CompletedTask;
            });
        });

        return services;
    }

    /// <summary>
    /// Publica o documento OpenAPI e a interface do Scalar.
    /// </summary>
    public static WebApplication UseOpenApiConfiguration(this WebApplication app)
    {
        // Documento cru, em /openapi/v1.json.
        app.MapOpenApi();

        // Interface interativa em /scalar. E daqui que voce dispara as
        // requisicoes para debugar as rotas.
        app.MapScalarApiReference(options =>
        {
            options.Title = "API de Gestão de Chamadas";
            options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        return app;
    }
}
