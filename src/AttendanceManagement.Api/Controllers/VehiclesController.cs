using AttendanceManagement.Application.Vehicles;
using AttendanceManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Api.Controllers;

/// <summary>Cadastro de veículos do transportador.</summary>
[Route("api/vehicles")]
[Authorize(Roles = nameof(PrimaryRole.Transporter))]
public sealed class VehiclesController(VehicleService service) : ApiController
{
    /// <summary>Lista os veículos do transportador.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<VehicleResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(TenantId, cancellationToken));

    /// <summary>Obtém um veículo pelo id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<VehicleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        FromResult(await service.GetByIdAsync(TenantId, id, cancellationToken));

    /// <summary>Cadastra um veículo.</summary>
    [HttpPost]
    [ProducesResponseType<VehicleResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateVehicleRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(TenantId, request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ToProblem(result.Error);
    }

    /// <summary>Atualiza um veículo.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<VehicleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, UpdateVehicleRequest request, CancellationToken cancellationToken) =>
        FromResult(await service.UpdateAsync(TenantId, id, request, cancellationToken));

    /// <summary>Remove um veículo (soft delete).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        NoContentOrProblem(await service.DeleteAsync(TenantId, id, cancellationToken));
}
