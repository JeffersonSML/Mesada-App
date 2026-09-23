using Mesada.Api.Contracts;
using Mesada.Application.Auth;
using Mesada.Application.Exceptions;
using Mesada.Application.Notificacoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

/// <summary>
/// Destinatários extras (e-mail/telefone) que recebem as notificações da
/// família, além do cadastro principal do Master/Comum — tela de
/// configuração do Master (docs/especificacao.md#notificações). Não
/// confundir com credenciais dos provedores (Resend/FCM), que são segredo
/// de infraestrutura configurado via ambiente, nunca por endpoint.
/// </summary>
[ApiController]
[Route("api/notificacoes/destinatarios")]
[Authorize(Roles = MesadaPapeis.Master)]
public sealed class NotificacoesController(
    ListarDestinatariosNotificacaoUseCase listar,
    AdicionarDestinatarioNotificacaoUseCase adicionar,
    RemoverDestinatarioNotificacaoUseCase remover) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DestinatarioNotificacaoResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var destinatarios = await listar.ExecutarAsync(ct);
        var resposta = destinatarios.Select(d => new DestinatarioNotificacaoResponse(d.Id, d.Tipo, d.Valor, d.Ativo));
        return Ok(resposta);
    }

    [HttpPost]
    [ProducesResponseType<DestinatarioNotificacaoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Adicionar([FromBody] AdicionarDestinatarioNotificacaoRequest request, CancellationToken ct)
    {
        try
        {
            var destinatario = await adicionar.ExecutarAsync(request.Tipo, request.Valor, ct);
            var resposta = new DestinatarioNotificacaoResponse(destinatario.Id, destinatario.Tipo, destinatario.Valor, destinatario.Ativo);
            return CreatedAtAction(nameof(Listar), null, resposta);
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
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
}
