using AttendanceManagement.Application.Transporters;
using AttendanceManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Api.Controllers;

/// <summary>Endpoints do transportador.</summary>
[Route("api/transporters")]
[Authorize(Roles = nameof(PrimaryRole.Transporter))]
public sealed class TransportersController(TransportTeamService service) : ApiController
{
    /// <summary>
    /// Equipe de transporte do motorista logado: ele e seus grupos, cada um com o
    /// monitor designado, o veículo e os alunos matriculados. O tenant vem da claim
    /// <c>sub</c> do token — nada de id na rota.
    /// </summary>
    [HttpGet("me/team")]
    [ProducesResponseType<TransportTeamEnvelope>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyTeam(CancellationToken cancellationToken)
    {
        var result = await service.GetByUserAccountAsync(CurrentUserId, cancellationToken);

        return result.IsSuccess
            ? Ok(new TransportTeamEnvelope(result.Value))
            : ToProblem(result.Error);
    }
}
