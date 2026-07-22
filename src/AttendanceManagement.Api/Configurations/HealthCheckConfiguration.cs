using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AttendanceManagement.Api.Configurations;

/// <summary>
/// Dois endpoints de saude, com papeis diferentes:
///
/// /health       - liveness. O processo esta de pe? Nao toca no banco.
/// /health/ready - readiness. Da para atender requisicao de verdade? Consulta
///                 o PostgreSQL. E este que prova que a conexao com o
///                 Clever Cloud esta funcionando.
///
/// A distincao importa em deploy: um orquestrador reinicia o container quando
/// liveness falha, mas apenas tira do balanceador quando readiness falha.
/// Se o banco cair, reiniciar a API nao resolve nada.
///
/// O check do banco em si e registrado no AddInfrastructure, porque so aquela
/// camada conhece o AppDbContext.
/// </summary>
internal static class HealthCheckConfiguration
{
    public static WebApplication UseHealthCheckConfiguration(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            // Nenhum check: responde 200 se o processo esta vivo.
            Predicate = _ => false,
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
            ResponseWriter = WriteHealthResponseAsync,
        });

        return app;
    }

    /// <summary>
    /// Resposta JSON com o detalhe de cada check, em vez do texto puro
    /// "Healthy" que o ASP.NET Core devolve por padrao.
    /// </summary>
    private static Task WriteHealthResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = entry.Value.Duration.TotalMilliseconds,
                error = entry.Value.Exception?.Message,
            }),
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
