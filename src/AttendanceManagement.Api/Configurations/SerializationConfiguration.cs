using System.Text.Json.Serialization;

namespace AttendanceManagement.Api.Configurations;

/// <summary>Controllers + politica de serializacao JSON.</summary>
internal static class SerializationConfiguration
{
    public static IServiceCollection AddControllersConfiguration(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
                // Nao serializa propriedade nula: payload menor na rede.
                //
                // A serializacao source-generated (JsonSerializerContext) volta
                // aqui quando os DTOs existirem — insere-se o contexto no topo do
                // TypeInfoResolverChain para eliminar reflection no hot path.
                options.JsonSerializerOptions.DefaultIgnoreCondition =
                    JsonIgnoreCondition.WhenWritingNull);

        return services;
    }
}
