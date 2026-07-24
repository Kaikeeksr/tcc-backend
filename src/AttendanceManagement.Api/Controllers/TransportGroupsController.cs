using AttendanceManagement.Application.TransportGroups;
using AttendanceManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Api.Controllers;

/// <summary>Cadastro dos grupos de transporte (as "turmas da van") do transportador.</summary>
[Route("api/transport-groups")]
[Authorize(Roles = nameof(PrimaryRole.Transporter))]
public sealed class TransportGroupsController(TransportGroupService service) : ApiController
{
    /// <summary>Lista os grupos do transportador.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TransportGroupResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(TenantId, cancellationToken));

    /// <summary>Obtém um grupo pelo id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<TransportGroupResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        FromResult(await service.GetByIdAsync(TenantId, id, cancellationToken));

    /// <summary>Cadastra um grupo.</summary>
    [HttpPost]
    [ProducesResponseType<TransportGroupResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateTransportGroupRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(TenantId, request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ToProblem(result.Error);
    }

    /// <summary>Atualiza nome/turno de um grupo.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<TransportGroupResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateTransportGroupRequest request, CancellationToken cancellationToken) =>
        FromResult(await service.UpdateAsync(TenantId, id, request, cancellationToken));

    /// <summary>Designa (ou desfaz) veículo e monitor do grupo.</summary>
    [HttpPut("{id:guid}/crew")]
    [ProducesResponseType<TransportGroupResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignCrew(Guid id, AssignCrewRequest request, CancellationToken cancellationToken) =>
        FromResult(await service.AssignCrewAsync(TenantId, id, request, cancellationToken));

    /// <summary>Remove um grupo (soft delete). Bloqueado se houver matrícula ativa.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        NoContentOrProblem(await service.DeleteAsync(TenantId, id, cancellationToken));
}
