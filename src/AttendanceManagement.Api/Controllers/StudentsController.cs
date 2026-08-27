using AttendanceManagement.Application.Students;
using AttendanceManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Api.Controllers;

/// <summary>Cadastro de alunos do transportador.</summary>
[Route("api/students")]
[Authorize(Roles = nameof(PrimaryRole.Transporter))]
public sealed class StudentsController(StudentService service) : ApiController
{
    /// <summary>Lista os alunos do transportador.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<StudentResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(TenantId, cancellationToken));

    /// <summary>Obtém um aluno pelo id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<StudentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        FromResult(await service.GetByIdAsync(TenantId, id, cancellationToken));

    /// <summary>Cadastra um aluno.</summary>
    [HttpPost]
    [ProducesResponseType<StudentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(TenantId, request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ToProblem(result.Error);
    }

    /// <summary>Atualiza um aluno.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<StudentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateStudentRequest request, CancellationToken cancellationToken) =>
        FromResult(await service.UpdateAsync(TenantId, id, request, cancellationToken));

    /// <summary>Cria o login opcional do aluno, para ele acompanhar a própria frequência no app.</summary>
    [HttpPost("{id:guid}/login")]
    [ProducesResponseType<StudentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateLogin(Guid id, CreateStudentLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateLoginAsync(TenantId, id, request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ToProblem(result.Error);
    }

    /// <summary>Remove um aluno (soft delete). Bloqueado se houver matrícula ou vínculo ativo.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        NoContentOrProblem(await service.DeleteAsync(TenantId, id, cancellationToken));
}
