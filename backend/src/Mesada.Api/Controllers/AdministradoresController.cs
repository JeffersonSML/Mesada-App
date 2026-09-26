using Mesada.Api.Contracts;
using Mesada.Application.Administracao;
using Mesada.Application.Auth;
using Mesada.Application.Exceptions;
using Mesada.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

/// <summary>
/// Gestão de quem tem acesso ao painel administrativo interno — restrita ao
/// grupo Owner (ver ExigirOwner na Application). Qualquer administrador
/// autenticado pode listar, mas só o Owner convida, edita ou desativa.
/// </summary>
[ApiController]
[Route("api/admin/administradores")]
[Authorize(Roles = MesadaPapeis.Administrador)]
public sealed class AdministradoresController(
    ListarAdministradoresUseCase listar,
    ConvidarAdministradorUseCase convidar,
    AtualizarAdministradorUseCase atualizar,
    DesativarAdministradorUseCase desativar) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AdministradorResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var administradores = await listar.ExecutarAsync(ct);
        return Ok(administradores.Select(ParaResposta));
    }

    [HttpPost]
    [ProducesResponseType<ConviteAdministradorResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Convidar([FromBody] ConvidarAdministradorRequest request, CancellationToken ct)
    {
        try
        {
            var resultado = await convidar.ExecutarAsync(request.Nome, request.Email, request.GrupoId, ct);
            var resposta = new ConviteAdministradorResponse(ParaResposta(resultado.Administrador), resultado.SenhaTemporaria);
            return CreatedAtAction(nameof(Listar), null, resposta);
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
        catch (EmailJaCadastradoException ex)
        {
            return Conflict(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status409Conflict });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<AdministradorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarAdministradorRequest request, CancellationToken ct)
    {
        try
        {
            var administrador = await atualizar.ExecutarAsync(id, request.Nome, request.GrupoId, ct);
            return Ok(ParaResposta(administrador));
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
    public async Task<IActionResult> Desativar(Guid id, CancellationToken ct)
    {
        try
        {
            await desativar.ExecutarAsync(id, ct);
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

    private static AdministradorResponse ParaResposta(Administrador a) =>
        new(a.Id, a.Nome, a.Email, a.Status, a.DeveTrocarSenha, a.GrupoId, a.Grupo?.Nome);
}
