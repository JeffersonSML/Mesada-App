using Mesada.Api.Contracts;
using Mesada.Application.Auth;
using Mesada.Application.Execucoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

/// <summary>Tarefas do próprio filho autenticado (app mobile do Comum) — rota separada de TarefasController, que é exclusivo do Master.</summary>
[ApiController]
[Route("api/tarefas/minhas")]
[Authorize(Roles = MesadaPapeis.Comum)]
public sealed class MinhasTarefasController(ListarMinhasTarefasUseCase listar) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MinhaTarefaResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var aderencias = await listar.ExecutarAsync(ct);
        var resposta = aderencias.Select(a => new MinhaTarefaResponse(
            a.Id, a.TarefaId, a.Tarefa!.Nome, a.Tarefa.Descricao, a.Tarefa.ModoCalculo,
            a.ValorOverride ?? a.Tarefa.Valor, a.PontosOverride ?? a.Tarefa.Pontos,
            a.Tarefa.PermiteParcial, a.Tarefa.RequerAprovacao));
        return Ok(resposta);
    }
}
