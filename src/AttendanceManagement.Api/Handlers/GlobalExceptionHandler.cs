using AttendanceManagement.Application.Abstractions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Api.Handlers;

/// <summary>
/// Rede de segurança para o que escapou do Result pattern: banco fora do ar, bug,
/// corrida de concorrência. Regra de negócio previsível volta como Result e nunca
/// chega aqui.
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    // 499 é a convenção do nginx para "cliente fechou a conexão antes da resposta".
    private const int Status499ClientClosedRequest = 499;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            UniqueConstraintViolationException => (
                StatusCodes.Status409Conflict,
                "Conflict with current state",
                "The record already exists. Check the unique fields and try again."),

            OperationCanceledException => (
                Status499ClientClosedRequest,
                "Request canceled",
                "The request was canceled by the client."),

            // Detalhe genérico de propósito: a mensagem da exceção pode conter
            // nome de tabela, caminho de arquivo ou trecho de connection string.
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal error",
                "An unexpected error occurred while processing the request."),
        };

        logger.LogError(
            exception,
            "Exceção não tratada em {Method} {Path} (status {StatusCode})",
            httpContext.Request.Method,
            httpContext.Request.Path,
            statusCode);

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Type = $"https://httpstatuses.io/{statusCode}",
                Instance = httpContext.Request.Path,
            },
        });
    }
}
