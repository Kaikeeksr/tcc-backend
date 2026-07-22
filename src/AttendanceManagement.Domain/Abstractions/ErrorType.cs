namespace AttendanceManagement.Domain.Abstractions;

/// <summary>
/// Classifica a natureza de um erro de dominio.
/// A camada de API traduz cada valor para um status HTTP, sem que o dominio
/// precise conhecer HTTP. Ver ApiController.HandleResult.
/// </summary>
public enum ErrorType
{
    /// <summary>Falha inesperada. Vira 500.</summary>
    Failure = 0,

    /// <summary>Entrada invalida segundo a regra de negocio. Vira 400.</summary>
    Validation = 1,

    /// <summary>Recurso nao encontrado. Vira 404.</summary>
    NotFound = 2,

    /// <summary>Conflito com o estado atual (ex.: e-mail duplicado). Vira 409.</summary>
    Conflict = 3,
}
