using System.Diagnostics;
using System.Net.Mime;
using AttendanceManagement.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Api.Controllers;

/// <summary>
/// Base de todos os controllers.
///
/// Sua unica responsabilidade e traduzir <see cref="Result"/> (linguagem do
/// dominio) em status HTTP (linguagem da web). Concentrar isso aqui evita o
/// if-else de tratamento de erro espalhado por cada action.
///
/// E o equivalente moderno do MainController.CustomResponse do TemplateCamadas,
/// so que devolvendo ProblemDetails (RFC 9457) em vez de um envelope proprio —
/// assim qualquer cliente HTTP entende o erro sem ler documentacao sua.
/// </summary>
[ApiController]
[Produces(MediaTypeNames.Application.Json)]
public abstract class ApiController : ControllerBase
{
    /// <summary>
    /// Converte um <see cref="Error"/> de dominio em resposta HTTP.
    /// O mapeamento tipo-de-erro -> status vive aqui e em nenhum outro lugar.
    /// </summary>
    protected ObjectResult ToProblem(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Type switch
            {
                ErrorType.Validation => "Requisição inválida",
                ErrorType.NotFound => "Recurso não encontrado",
                ErrorType.Conflict => "Conflito com o estado atual",
                _ => "Erro interno",
            },
            Detail = error.Description,
            Type = $"https://httpstatuses.io/{statusCode}",
            Instance = HttpContext.Request.Path,
        };

        // Codigo estavel para o cliente tratar programaticamente, sem depender
        // do texto da mensagem (que pode mudar ou ser traduzido).
        problemDetails.Extensions["errorCode"] = error.Code;

        // Mesmo traceId que o ProblemDetails automatico do [ApiController] e o
        // GlobalExceptionHandler incluem. Sem isso, so parte dos erros da API
        // seria correlacionavel com o log do servidor.
        problemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode,
        };
    }
}
