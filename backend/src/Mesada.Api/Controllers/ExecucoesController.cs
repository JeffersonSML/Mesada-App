using Mesada.Api.Contracts;
using Mesada.Application.Auth;
using Mesada.Application.Exceptions;
using Mesada.Application.Execucoes;
using Mesada.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

/// <summary>Fluxo de conclusão e aprovação de tarefas (docs/especificacao.md#fluxo-de-aprovação-de-tarefas).</summary>
[ApiController]
[Route("api/execucoes")]
[Authorize]
public sealed class ExecucoesController(
    MarcarExecucaoUseCase marcar,
    AprovarExecucaoUseCase aprovar,
    RejeitarExecucaoUseCase rejeitar,
    ListarExecucoesPendentesUseCase listarPendentes) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = MesadaPapeis.Comum)]
    [ProducesResponseType<ExecucaoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Marcar([FromBody] MarcarExecucaoRequest request, CancellationToken ct)
    {
        try
        {
            var execucao = await marcar.ExecutarAsync(request.TarefaUsuarioId, request.Status, request.PercentualConclusao, ct);
            return CreatedAtAction(nameof(ListarPendentes), null, ParaResposta(execucao));
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

    [HttpPost("{id:guid}/aprovar")]
    [Authorize(Roles = MesadaPapeis.Master)]
    [ProducesResponseType<ExecucaoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Aprovar(Guid id, CancellationToken ct)
    {
        try
        {
            var execucao = await aprovar.ExecutarAsync(id, ct);
            return Ok(ParaResposta(execucao));
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

    [HttpPost("{id:guid}/rejeitar")]
    [Authorize(Roles = MesadaPapeis.Master)]
    [ProducesResponseType<ExecucaoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rejeitar(Guid id, CancellationToken ct)
    {
        try
        {
            var execucao = await rejeitar.ExecutarAsync(id, ct);
            return Ok(ParaResposta(execucao));
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

    [HttpGet("pendentes")]
    [Authorize(Roles = MesadaPapeis.Master)]
    [ProducesResponseType<IReadOnlyList<ExecucaoResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPendentes(CancellationToken ct)
    {
        var execucoes = await listarPendentes.ExecutarAsync(ct);
        return Ok(execucoes.Select(ParaResposta));
    }

    private static ExecucaoResponse ParaResposta(Execucao e) => new(
        e.Id, e.TarefaUsuarioId, e.DataExecucao, e.Status, e.PercentualConclusao, e.StatusAprovacao, e.ValorCalculado, e.PontosCalculado);
}
