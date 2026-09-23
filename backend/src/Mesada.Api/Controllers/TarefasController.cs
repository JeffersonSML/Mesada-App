using Mesada.Api.Contracts;
using Mesada.Application.Auth;
using Mesada.Application.Exceptions;
using Mesada.Application.Tarefas;
using Mesada.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

/// <summary>Cadastro e parametrização de tarefas — cadastro exclusivo da Web, restrito ao Master (docs/especificacao.md#cadastro-e-parametrização-de-tarefas).</summary>
[ApiController]
[Route("api/tarefas")]
[Authorize(Roles = MesadaPapeis.Master)]
public sealed class TarefasController(
    ListarTarefasUseCase listar,
    CriarTarefaUseCase criar,
    AtualizarTarefaUseCase atualizar,
    RemoverTarefaUseCase remover,
    AdicionarAderenciaUseCase adicionarAderencia,
    RemoverAderenciaUseCase removerAderencia,
    SugerirValorTarefaUseCase sugerirValor) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TarefaResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var tarefas = await listar.ExecutarAsync(ct);
        return Ok(tarefas.Select(ParaResposta));
    }

    [HttpGet("sugestao")]
    [ProducesResponseType<SugestaoValorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Sugerir([FromQuery] Guid usuarioComumId, [FromQuery] Mesada.Domain.Enums.ModoCalculo modoCalculo, CancellationToken ct)
    {
        try
        {
            var sugestao = await sugerirValor.ExecutarAsync(usuarioComumId, modoCalculo, ct);
            return Ok(new SugestaoValorResponse(sugestao.Valor, sugestao.ModoCalculo));
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

    [HttpPost]
    [ProducesResponseType<TarefaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Criar([FromBody] SalvarTarefaRequest request, CancellationToken ct)
    {
        try
        {
            var tarefa = await criar.ExecutarAsync(ParaDados(request), ct);
            return CreatedAtAction(nameof(Listar), null, ParaResposta(tarefa));
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
    [ProducesResponseType<TarefaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] SalvarTarefaRequest request, CancellationToken ct)
    {
        try
        {
            var tarefa = await atualizar.ExecutarAsync(id, ParaDados(request), ct);
            return Ok(ParaResposta(tarefa));
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        try
        {
            await remover.ExecutarAsync(id, ct);
            return NoContent();
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status404NotFound });
        }
    }

    [HttpPost("{id:guid}/aderencias")]
    [ProducesResponseType<AderenciaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdicionarAderencia(Guid id, [FromBody] AderenciaRequest request, CancellationToken ct)
    {
        try
        {
            var aderencia = await adicionarAderencia.ExecutarAsync(id, request.UsuarioComumId, request.ValorOverride, request.PontosOverride, ct);
            var resposta = new AderenciaResponse(aderencia.TarefaId, aderencia.UsuarioComumId, aderencia.ValorOverride, aderencia.PontosOverride);
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

    [HttpDelete("{id:guid}/aderencias/{usuarioComumId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverAderencia(Guid id, Guid usuarioComumId, CancellationToken ct)
    {
        try
        {
            await removerAderencia.ExecutarAsync(id, usuarioComumId, ct);
            return NoContent();
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status404NotFound });
        }
    }

    private static DadosTarefa ParaDados(SalvarTarefaRequest r) => new(
        r.CategoriaId, r.Nome, r.Descricao, r.ModoCalculo, r.Valor, r.Pontos, r.PermiteParcial, r.Tipo,
        r.ValidadeAvulsa, r.Natureza, r.ValorMulta, r.RequerAprovacao, r.TipoEvidencia, r.ProvedorIntegracao);

    private static TarefaResponse ParaResposta(Tarefa t) => new(
        t.Id, t.CategoriaId, t.Nome, t.Descricao, t.ModoCalculo, t.Valor, t.Pontos, t.PermiteParcial, t.Tipo,
        t.ValidadeAvulsa, t.Natureza, t.ValorMulta, t.RequerAprovacao, t.TipoEvidencia, t.ProvedorIntegracao);
}
