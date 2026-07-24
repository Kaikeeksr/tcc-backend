using System.Security.Claims;
using AttendanceManagement.Application.Authentication;
using AttendanceManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Api.Controllers;

/// <summary>Autenticação: cadastro do transportador, login e "quem sou eu".</summary>
[Route("api/auth")]
public sealed class AuthController(AuthenticationService service) : ApiController
{
    /// <summary>Cadastra o transportador (raiz do tenant) e já devolve o token.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterTransporterRequest request, CancellationToken cancellationToken) =>
        FromResult(await service.RegisterTransporterAsync(request, cancellationToken));

    /// <summary>Troca e-mail + senha por um JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken) =>
        FromResult(await service.LoginAsync(request, cancellationToken));

    /// <summary>Devolve o usuário do token atual, lendo direto das claims (sem tocar o banco).</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<AuthenticatedUser>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        // Se qualquer claim essencial faltar, o token não foi emitido por nós.
        if (!TryReadGuid(AuthClaims.Subject, out var id) ||
            !TryReadGuid(AuthClaims.ProfileId, out var profileId) ||
            !TryReadGuid(AuthClaims.TransporterId, out var transporterId) ||
            !Enum.TryParse<PrimaryRole>(User.FindFirstValue(AuthClaims.Role), out var role))
        {
            return Unauthorized();
        }

        var user = new AuthenticatedUser(
            id,
            User.FindFirstValue(AuthClaims.Name) ?? string.Empty,
            User.FindFirstValue(AuthClaims.Email) ?? string.Empty,
            role,
            profileId,
            transporterId);

        return Ok(user);
    }

    private bool TryReadGuid(string claimType, out Guid value) =>
        Guid.TryParse(User.FindFirstValue(claimType), out value);
}
