using System.Reflection;
using System.Text.Json.Serialization;
using AttendanceManagement.Application.Assistants;
using AttendanceManagement.Application.Attendance;
using AttendanceManagement.Application.Authentication;
using AttendanceManagement.Application.CalendarDays;
using AttendanceManagement.Application.Enrollments;
using AttendanceManagement.Application.Guardians;
using AttendanceManagement.Application.GuardianStudents;
using AttendanceManagement.Application.Schools;
using AttendanceManagement.Application.Students;
using AttendanceManagement.Application.TransportGroups;
using AttendanceManagement.Application.Transporters;
using AttendanceManagement.Application.Vehicles;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AttendanceManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<TransportTeamService>();
        services.AddScoped<AuthenticationService>();
        services.AddScoped<VehicleService>();
        services.AddScoped<SchoolService>();
        services.AddScoped<TransportGroupService>();
        services.AddScoped<StudentService>();
        services.AddScoped<AssistantService>();
        services.AddScoped<GuardianService>();
        services.AddScoped<EnrollmentService>();
        services.AddScoped<GuardianStudentService>();
        services.AddScoped<AttendanceService>();
        services.AddScoped<CalendarDayService>();

        // Todos os AbstractValidator<> desta assembly viram IValidator<T> escopados.
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Nomes de campo nos erros de validação saem em snake_case (o JsonPropertyName
        // do contrato), casando com o corpo que o cliente enviou.
        ValidatorOptions.Global.PropertyNameResolver = (_, member, _) =>
            member?.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? member?.Name;

        return services;
    }
}
