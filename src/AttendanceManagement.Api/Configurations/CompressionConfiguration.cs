using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;

namespace AttendanceManagement.Api.Configurations;

/// <summary>
/// Compressao de resposta: Brotli com Gzip de reserva.
///
/// Uma lista JSON comprime muito bem (texto repetitivo). Brotli costuma render
/// 15-20% a mais que Gzip em clientes modernos; navegador antigo cai no Gzip
/// automaticamente pelo cabecalho Accept-Encoding.
/// </summary>
internal static class CompressionConfiguration
{
    public static IServiceCollection AddCompressionConfiguration(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            // Desligado por padrao no ASP.NET Core por causa do ataque BREACH,
            // que explora resposta comprimida misturando segredo com entrada
            // refletida do usuario. Esta API nao devolve token nem segredo no
            // corpo, entao o risco nao se aplica e a economia de banda e real.
            // Se um dia um endpoint passar a devolver dado sensivel refletido,
            // reavalie esta linha.
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

        // Fastest, nao Optimal: para payload de API o ganho extra do Optimal e
        // pequeno e o custo de CPU por requisicao e alto. Comprimir rapido e
        // devolver antes vence.
        services.Configure<BrotliCompressionProviderOptions>(
            options => options.Level = CompressionLevel.Fastest);

        services.Configure<GzipCompressionProviderOptions>(
            options => options.Level = CompressionLevel.Fastest);

        return services;
    }
}
