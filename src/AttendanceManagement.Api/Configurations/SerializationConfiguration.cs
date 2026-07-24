using System.Text.Json.Serialization;
using AttendanceManagement.Api.Filters;

namespace AttendanceManagement.Api.Configurations;

/// <summary>Controllers + política de serialização JSON.</summary>
internal static class SerializationConfiguration
{
    public static IServiceCollection AddControllersConfiguration(this IServiceCollection services)
    {
        services
            .AddControllers(options => options.Filters.Add<ValidationFilter>())
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition =
                    JsonIgnoreCondition.WhenWritingNull;

                // Enum como texto ("Mother"), não o inteiro 1 — igual ao que o banco grava.
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        return services;
    }
}
