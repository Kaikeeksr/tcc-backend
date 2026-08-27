using AttendanceManagement.Application.Abstractions;
using AttendanceManagement.Infrastructure.Authentication;
using AttendanceManagement.Infrastructure.Persistence;
using AttendanceManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace AttendanceManagement.Infrastructure;

/// <summary>
/// Composition root da persistência. É a única coisa que o Program.cs conhece da
/// Infrastructure; todo acoplamento com PostgreSQL vive aqui.
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
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' não configurada. " +
                "Rode: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<...>\" " +
                "--project src/AttendanceManagement.Api");
        }

        // Singleton porque é ele que mantém o pool de conexões.
        services.AddSingleton(_ => new NpgsqlDataSourceBuilder(connectionString).Build());

        services.AddDbContextPool<AppDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(
                serviceProvider.GetRequiredService<NpgsqlDataSource>(),
                npgsql => npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null));

            options.UseSnakeCaseNamingConvention();
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            // Dependentes históricos (chamada, event_log) sobrevivem de propósito
            // ao soft delete do principal; propagar o filtro para eles destruiria a
            // trilha de auditoria, então o warning é o comportamento desejado.
            options.ConfigureWarnings(warnings => warnings.Ignore(
                CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITransporterRepository, TransporterRepository>();
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<ITransportGroupRepository, TransportGroupRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IAssistantRepository, AssistantRepository>();
        services.AddScoped<IGuardianRepository, GuardianRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IGuardianStudentRepository, GuardianStudentRepository>();
        services.AddScoped<IAttendanceSessionRepository, AttendanceSessionRepository>();
        services.AddScoped<IAttendanceRecordRepository, AttendanceRecordRepository>();
        services.AddScoped<IEventLogRepository, EventLogRepository>();
        services.AddScoped<ICalendarDayRepository, CalendarDayRepository>();

        AddAuthentication(services, configuration);

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(
                name: "postgresql",
                tags: ["ready"]);

        return services;
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        // Valida a seção Jwt no boot. Binding pelo indexador (não .Get<T>()) para
        // não puxar o pacote do binder nesta camada não-web.
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = new JwtOptions
        {
            Issuer = jwtSection["Issuer"] ?? string.Empty,
            Audience = jwtSection["Audience"] ?? string.Empty,
            SigningKey = jwtSection["SigningKey"] ?? string.Empty,
            ExpirationMinutes = int.TryParse(jwtSection["ExpirationMinutes"], out var minutes) ? minutes : 120,
        };
        jwtOptions.Validate();
        services.AddSingleton(Options.Create(jwtOptions));

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
    }
}
