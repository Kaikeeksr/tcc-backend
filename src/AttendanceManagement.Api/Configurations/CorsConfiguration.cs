namespace AttendanceManagement.Api.Configurations;

/// <summary>
/// CORS dirigido por configuracao.
///
/// O TemplateCamadas usava AllowAnyOrigin fixo no codigo. Funciona em
/// desenvolvimento e vira problema em producao, onde qualquer site passaria a
/// poder chamar a API pelo navegador do seu usuario. Aqui as origens vem do
/// appsettings, entao dev e producao se configuram sem recompilar.
/// </summary>
internal static class CorsConfiguration
{
    public const string PolicyName = "DefaultCorsPolicy";

    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options => options.AddPolicy(PolicyName, policy =>
        {
            if (allowedOrigins.Length == 0)
            {
                // Sem origem configurada (caso do ambiente local): libera geral.
                // AllowAnyOrigin e AllowCredentials sao mutuamente exclusivos —
                // o navegador rejeita a combinacao.
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
