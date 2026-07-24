using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;

namespace AttendanceManagement.Api.Configurations;

/// <summary>Compressão de resposta: Brotli com Gzip de reserva.</summary>
internal static class CompressionConfiguration
{
    public static IServiceCollection AddCompressionConfiguration(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            // Desligado por padrão por causa do ataque BREACH. Esta API não devolve
            // token nem segredo no corpo, então o risco não se aplica. Reavalie se
            // um endpoint passar a devolver dado sensível refletido.
            options.EnableForHttps = true;

            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();

            options.MimeTypes =
            [
                .. ResponseCompressionDefaults.MimeTypes,
                "application/json",
                "application/problem+json",
            ];
        });

        // Fastest: para payload de API, o ganho do Optimal é pequeno e o custo de CPU alto.
        services.Configure<BrotliCompressionProviderOptions>(
            options => options.Level = CompressionLevel.Fastest);

        services.Configure<GzipCompressionProviderOptions>(
            options => options.Level = CompressionLevel.Fastest);

        return services;
    }
}
