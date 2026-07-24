namespace AttendanceManagement.Api.Configurations;

/// <summary>CORS dirigido por configuração (seção <c>Cors:AllowedOrigins</c>).</summary>
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
