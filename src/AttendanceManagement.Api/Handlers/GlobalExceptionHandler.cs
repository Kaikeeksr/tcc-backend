using AttendanceManagement.Application.Abstractions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Api.Handlers;

/// <summary>
/// Rede de seguranca para o que escapou do Result pattern.
///
/// Regra de negocio previsivel volta como Result e nunca chega aqui. O que chega
/// aqui e o inesperado: banco fora do ar, bug, corrida de concorrencia.
///
/// O TemplateCamadas nao tinha nada disso — excecao nao tratada em Producao
/// virava um 500 vazio, sem corpo e sem rastro.
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>
    /// 499 nao existe na classe StatusCodes do ASP.NET Core; e a convencao do
    /// nginx para "o cliente fechou a conexao antes da resposta".
    /// </summary>
    private const int Status499ClientClosedRequest = 499;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            // Corrida perdida para o indice unico do banco: e conflito, nao bug.
            UniqueConstraintViolationException => (
                StatusCodes.Status409Conflict,
                "Conflito com o estado atual",
                "O registro já existe. Verifique os campos únicos e tente novamente."),

            OperationCanceledException => (
                Status499ClientClosedRequest,
                "Requisição cancelada",
                "A requisição foi cancelada pelo cliente."),

            // Detalhe generico de proposito: mensagem de excecao pode conter
            // nome de tabela, caminho de arquivo ou trecho de connection string.
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno",
                "Ocorreu um erro inesperado ao processar a requisição."),
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
