using AttendanceManagement.Application.Schools;
using AttendanceManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Api.Controllers;

/// <summary>Cadastro de escolas do transportador.</summary>
[Route("api/schools")]
[Authorize(Roles = nameof(PrimaryRole.Transporter))]
public sealed class SchoolsController(SchoolService service) : ApiController
{
    /// <summary>Lista as escolas do transportador.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<SchoolResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(TenantId, cancellationToken));

    /// <summary>Obtém uma escola pelo id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<SchoolResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        FromResult(await service.GetByIdAsync(TenantId, id, cancellationToken));

    /// <summary>Cadastra uma escola.</summary>
    [HttpPost]
    [ProducesResponseType<SchoolResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateSchoolRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(TenantId, request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ToProblem(result.Error);
    }

    /// <summary>Atualiza uma escola.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<SchoolResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, UpdateSchoolRequest request, CancellationToken cancellationToken) =>
        FromResult(await service.UpdateAsync(TenantId, id, request, cancellationToken));

    /// <summary>Remove uma escola (soft delete).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        NoContentOrProblem(await service.DeleteAsync(TenantId, id, cancellationToken));
}
