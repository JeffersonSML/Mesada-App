using System.Text.Json;
using Mesada.Api.Contracts;
using Mesada.Application.Administracao;
using Mesada.Application.Auth;
using Mesada.Application.Exceptions;
using Mesada.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

/// <summary>Grupos de acesso do painel administrativo (ex.: "Tecnologia") — criação/edição/remoção restritas ao grupo Owner.</summary>
[ApiController]
[Route("api/admin/grupos")]
[Authorize(Roles = MesadaPapeis.Administrador)]
public sealed class GruposAdministradorController(
    ListarGruposAdministradorUseCase listar,
    CriarGrupoAdministradorUseCase criar,
    AtualizarGrupoAdministradorUseCase atualizar,
    RemoverGrupoAdministradorUseCase remover) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<GrupoAdministradorResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var grupos = await listar.ExecutarAsync(ct);
        return Ok(grupos.Select(ParaResposta));
    }

    [HttpPost]
    [ProducesResponseType<GrupoAdministradorResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Criar([FromBody] SalvarGrupoAdministradorRequest request, CancellationToken ct)
    {
        try
        {
            var grupo = await criar.ExecutarAsync(request.Nome, request.Descricao, request.Permissoes, ct);
            return CreatedAtAction(nameof(Listar), null, ParaResposta(grupo));
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
        catch (PermissaoNegadaException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status403Forbidden });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<GrupoAdministradorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] SalvarGrupoAdministradorRequest request, CancellationToken ct)
    {
        try
        {
            var grupo = await atualizar.ExecutarAsync(id, request.Nome, request.Descricao, request.Permissoes, ct);
            return Ok(ParaResposta(grupo));
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
        catch (PermissaoNegadaException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status403Forbidden });
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status404NotFound });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        catch (PermissaoNegadaException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status403Forbidden });
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status404NotFound });
        }
    }

    private static GrupoAdministradorResponse ParaResposta(GrupoAdministrador g) => new(
        g.Id, g.Nome, g.Descricao,
        JsonSerializer.Deserialize<Dictionary<string, bool>>(g.Permissoes) ?? new Dictionary<string, bool>(),
        g.Sistema);
}
