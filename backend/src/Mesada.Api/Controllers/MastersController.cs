using Mesada.Api.Contracts;
using Mesada.Application.Auth;
using Mesada.Application.Exceptions;
using Mesada.Application.Familias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

/// <summary>Segundo(s) responsável(is) da família — cadastro é via convite (POST /api/convites com papelAlvo=Master).</summary>
[ApiController]
[Route("api/masters")]
[Authorize(Roles = MesadaPapeis.Master)]
public sealed class MastersController(
    ListarMastersUseCase listar,
    DesativarMasterUseCase desativar) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MasterResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var masters = await listar.ExecutarAsync(ct);
        var resposta = masters.Select(m => new MasterResponse(m.Id, m.Nome, m.Email, m.IsFinanceiro, m.Status));
        return Ok(resposta);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status404NotFound });
        }
    }
}
