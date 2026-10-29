namespace AttendanceManagement.Api.Configurations;

/// <summary>CORS dirigido por configuração (seção <c>Cors:AllowedOrigins</c>).</summary>
internal static class CorsConfiguration
{
    public const string PolicyName = "DefaultCorsPolicy";

    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        if (allowedOrigins.Length == 0 && environment.IsProduction())
        {
            // Travar aqui no boot é melhor que subir em produção aceitando qualquer origem
            // por configuração esquecida.
            throw new InvalidOperationException(
                "Cors:AllowedOrigins precisa ser configurado em produção (env var Cors__AllowedOrigins__0, __1, ...).");
        }

        services.AddCors(options => options.AddPolicy(PolicyName, policy =>
        {
            if (allowedOrigins.Length == 0)
            {
                // Sem origem configurada (ambiente local): libera geral. AllowAnyOrigin
                // e AllowCredentials são mutuamente exclusivos.
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            }
            else
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            }
        }));

        return services;
    }
}
