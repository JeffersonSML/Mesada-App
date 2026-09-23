using Mesada.Api.Contracts;
using Mesada.Application.Auth;
using Mesada.Application.Convites;
using Mesada.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

/// <summary>Gestão de convites de acesso (docs/especificacao.md#fluxo-de-convite) — o código gerado aqui é resgatado pelo app mobile via POST /api/auth/convites/{codigo}/resgatar.</summary>
[ApiController]
[Route("api/convites")]
[Authorize(Roles = MesadaPapeis.Master)]
public sealed class ConvitesController(
    ListarConvitesUseCase listar,
    CriarConviteUseCase criar,
    RevogarConviteUseCase revogar) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ConviteResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var convites = await listar.ExecutarAsync(ct);
        var resposta = convites.Select(ParaResposta);
        return Ok(resposta);
    }

    [HttpPost]
    [ProducesResponseType<ConviteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Criar([FromBody] CriarConviteRequest request, CancellationToken ct)
    {
        try
        {
            var convite = await criar.ExecutarAsync(request.UsuarioComumId, ct);
            return CreatedAtAction(nameof(Listar), null, ParaResposta(convite));
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
    public async Task<IActionResult> Revogar(Guid id, CancellationToken ct)
    {
        try
        {
            await revogar.ExecutarAsync(id, ct);
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

    private static ConviteResponse ParaResposta(Mesada.Domain.Entities.ConviteAcesso c) =>
        new(c.Id, c.Codigo, c.PapelAlvo, c.UsuarioComumId, c.Status, c.ExpiraEm);
}
