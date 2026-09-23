using Mesada.Api.Contracts;
using Mesada.Application.Auth;
using Mesada.Application.Categorias;
using Mesada.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

/// <summary>Categorias de tarefas — modelo híbrido: defaults do sistema (somente leitura aqui) + customizadas da família (docs/adr/0002-schema-e-rls.md).</summary>
[ApiController]
[Route("api/categorias")]
[Authorize(Roles = MesadaPapeis.Master)]
public sealed class CategoriasController(
    ListarCategoriasUseCase listar,
    CriarCategoriaUseCase criar,
    AtualizarCategoriaUseCase atualizar,
    RemoverCategoriaUseCase remover) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CategoriaResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var categorias = await listar.ExecutarAsync(ct);
        var resposta = categorias.Select(c => new CategoriaResponse(c.Id, c.ParentId, c.Nome, c.Sistema));
        return Ok(resposta);
    }

    [HttpPost]
    [ProducesResponseType<CategoriaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Criar([FromBody] CriarCategoriaRequest request, CancellationToken ct)
    {
        try
        {
            var categoria = await criar.ExecutarAsync(request.Nome, request.ParentId, ct);
            var resposta = new CategoriaResponse(categoria.Id, categoria.ParentId, categoria.Nome, categoria.Sistema);
            return CreatedAtAction(nameof(Listar), null, resposta);
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status404NotFound });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<CategoriaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarCategoriaRequest request, CancellationToken ct)
    {
        try
        {
            var categoria = await atualizar.ExecutarAsync(id, request.Nome, ct);
            return Ok(new CategoriaResponse(categoria.Id, categoria.ParentId, categoria.Nome, categoria.Sistema));
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status404NotFound });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        try
        {
            await remover.ExecutarAsync(id, ct);
            return NoContent();
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status404NotFound });
        }
    }
}
