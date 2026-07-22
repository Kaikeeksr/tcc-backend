using Microsoft.Extensions.DependencyInjection;

namespace AttendanceManagement.Application;

/// <summary>
/// Registro dos servicos desta camada.
///
/// Manter isto aqui (e nao no Program.cs) significa que a Api nao precisa saber
/// quais classes concretas existem na Application. Cada caso de uso novo
/// (services) e registrado aqui, nao no composition root.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Ainda sem casos de uso: o schema do dominio foi implementado primeiro.
        // Os services (scoped, um por requisicao) entram aqui conforme a
        // superficie da API for construida.
        return services;
    }
}
