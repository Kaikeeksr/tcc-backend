using AttendanceManagement.Application.Assistants;
using AttendanceManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Api.Controllers;

/// <summary>Cadastro dos monitores (com login) do transportador.</summary>
[Route("api/assistants")]
[Authorize(Roles = nameof(PrimaryRole.Transporter))]
public sealed class AssistantsController(AssistantService service) : ApiController
{
    /// <summary>Lista os monitores do transportador.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AssistantResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(TenantId, cancellationToken));

    /// <summary>Obtém um monitor pelo id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<AssistantResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        FromResult(await service.GetByIdAsync(TenantId, id, cancellationToken));

    /// <summary>Cadastra um monitor e sua conta de login.</summary>
    [HttpPost]
    [ProducesResponseType<AssistantResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateAssistantRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(TenantId, request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ToProblem(result.Error);
    }

    /// <summary>Atualiza o nome de um monitor.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<AssistantResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateAssistantRequest request, CancellationToken cancellationToken) =>
        FromResult(await service.UpdateAsync(TenantId, id, request, cancellationToken));

    /// <summary>Remove um monitor (soft delete) e bloqueia seu login. Bloqueado se designado a um grupo.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        NoContentOrProblem(await service.DeleteAsync(TenantId, id, cancellationToken));
}
