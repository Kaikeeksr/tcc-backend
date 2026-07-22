using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace AttendanceManagement.Infrastructure;

/// <summary>
/// Composition root da persistencia.
///
/// Este metodo e a UNICA coisa que o Program.cs conhece da Infrastructure.
/// Trocar PostgreSQL por outro banco significa reescrever este arquivo e o
/// repositorio — nenhuma linha da Api, Application ou Domain muda.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Falhar no boot com mensagem clara e melhor do que falhar na primeira
            // requisicao com NullReference. O TemplateCamadas deixava a connection
            // string vazia e ninguem percebia ate o runtime.
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' não configurada. " +
                "Rode: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<...>\" " +
                "--project src/AttendanceManagement.Api");
        }

        // NpgsqlDataSource como singleton: e ele que mantem o pool de conexoes.
        // Criar um por requisicao jogaria fora o pooling e estouraria o limite
        // de conexoes do plano do Clever Cloud.
        services.AddSingleton(_ => new NpgsqlDataSourceBuilder(connectionString).Build());

        // AddDbContextPool reaproveita instancias de DbContext entre requisicoes
        // (reseta o estado interno em vez de alocar de novo).
        services.AddDbContextPool<AppDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(
                serviceProvider.GetRequiredService<NpgsqlDataSource>(),
                npgsql => npgsql.EnableRetryOnFailure(
                    // O banco esta na nuvem, do outro lado da internet: oscilacao
                    // de rede e normal e nao deveria virar erro para o usuario.
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null));

            // Tabelas e colunas em snake_case (attendance_record, created_at_utc)
            // em vez de PascalCase. E a convencao do PostgreSQL: sem isso, todo
            // identificador precisaria de aspas duplas ao consultar no psql/DBeaver.
            options.UseSnakeCaseNamingConvention();

            // Leitura sem change tracking por padrao: menos alocacao e nenhum
            // snapshot de entidade. Escrita nao e afetada — Add/Update continuam
            // rastreando normalmente.
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        // Repositorios concretos entram aqui conforme os casos de uso surgem.
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Health check do banco. Registrado aqui porque so esta camada conhece
        // o AppDbContext; a Api apenas expoe o endpoint /health/ready.
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(
                name: "postgresql",
                tags: ["ready"]);

        return services;
    }
}
