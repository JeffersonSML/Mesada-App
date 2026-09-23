using Mesada.Api.Contracts;
using Mesada.Application.Auth;
using Mesada.Application.Ciclos;
using Mesada.Application.Exceptions;
using Mesada.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

/// <summary>Fechamento e histórico de ciclos de mesada (docs/especificacao.md#ciclo-de-mesada).</summary>
[ApiController]
[Route("api/ciclos")]
[Authorize(Roles = MesadaPapeis.Master)]
public sealed class CiclosController(
    FecharCicloUseCase fechar,
    ListarHistoricoCiclosUseCase listarHistorico,
    ObterCicloAtualUseCase obterAtual) : ControllerBase
{
    [HttpPost("fechar")]
    [ProducesResponseType<CicloResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Fechar([FromBody] FecharCicloRequest request, CancellationToken ct)
    {
        try
        {
            var ciclo = await fechar.ExecutarAsync(request.UsuarioComumId, request.DataInicio, request.DataFim, ct);
            return CreatedAtAction(nameof(Historico), new { usuarioComumId = request.UsuarioComumId }, ParaResposta(ciclo));
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

    [HttpGet("historico")]
    [ProducesResponseType<IReadOnlyList<CicloResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Historico([FromQuery] Guid usuarioComumId, CancellationToken ct)
    {
        try
        {
            var ciclos = await listarHistorico.ExecutarAsync(usuarioComumId, ct);
            return Ok(ciclos.Select(ParaResposta));
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status404NotFound });
        }
    }

    [HttpGet("atual")]
    [ProducesResponseType<CicloAtualResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atual([FromQuery] Guid usuarioComumId, CancellationToken ct)
    {
        try
        {
            var previa = await obterAtual.ExecutarAsync(usuarioComumId, ct);
            return Ok(new CicloAtualResponse(
                previa.DataInicio, previa.DataFim, previa.MesadaBase, previa.SomaBonus, previa.SomaMultas,
                previa.SaldoDevedorAnterior, previa.ValorFinalPrevisto, previa.SaldoDevedorPrevisto));
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status404NotFound });
        }
    }

    private static CicloResponse ParaResposta(CicloMesada c) => new(
        c.Id, c.UsuarioComumId, c.DataInicio, c.DataFim, c.MesadaBase, c.SomaBonus, c.SomaMultas,
        c.SaldoDevedorAnterior, c.ValorFinal, c.SaldoDevedorResultante);
}
