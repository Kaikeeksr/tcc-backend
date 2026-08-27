using System.Diagnostics;
using System.Net.Mime;
using System.Security.Claims;
using AttendanceManagement.Application.Authentication;
using AttendanceManagement.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Api.Controllers;

/// <summary>
/// Base de todos os controllers. Traduz <see cref="Result"/> (domínio) em status
/// HTTP (web), devolvendo ProblemDetails (RFC 9457). O mapeamento tipo-de-erro →
/// status vive aqui e em nenhum outro lugar.
/// </summary>
[ApiController]
[Produces(MediaTypeNames.Application.Json)]
public abstract class ApiController : ControllerBase
{
    /// <summary>Tenant do token (claim <c>transporter_id</c>). Fonte única de escopo — nunca vem do corpo.</summary>
    protected Guid TenantId => ReadGuidClaim(AuthClaims.TransporterId);

    /// <summary>Conta de login do token (claim <c>sub</c>), para campos de auditoria.</summary>
    protected Guid CurrentUserId => ReadGuidClaim(AuthClaims.Subject);

    /// <summary>Id do perfil (guardian/student/assistant/transporter) do token (claim <c>profile_id</c>).</summary>
    protected Guid ProfileId => ReadGuidClaim(AuthClaims.ProfileId);

    private Guid ReadGuidClaim(string claimType) =>
        Guid.TryParse(User.FindFirstValue(claimType), out var value) ? value : Guid.Empty;

    /// <summary>Sucesso vira 200 com o valor; falha vira o ProblemDetails do erro.</summary>
    protected IActionResult FromResult<TValue>(Result<TValue> result) =>
        result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);

    /// <summary>Sucesso sem corpo vira 204; falha vira o ProblemDetails do erro.</summary>
    protected IActionResult NoContentOrProblem(Result result) =>
        result.IsSuccess ? NoContent() : ToProblem(result.Error);

    protected ObjectResult ToProblem(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Type switch
            {
                ErrorType.Validation => "Invalid request",
                ErrorType.Unauthorized => "Unauthorized",
                ErrorType.NotFound => "Resource not found",
                ErrorType.Conflict => "Conflict with current state",
                _ => "Internal error",
            },
            Detail = error.Description,
            Type = $"https://httpstatuses.io/{statusCode}",
            Instance = HttpContext.Request.Path,
        };

        // Código estável para o cliente tratar sem depender do texto da mensagem.
        problemDetails.Extensions["errorCode"] = error.Code;
        problemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode,
        };
    }
}
