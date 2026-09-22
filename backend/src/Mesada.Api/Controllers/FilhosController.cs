using Mesada.Api.Contracts;
using Mesada.Application.Auth;
using Mesada.Application.Familias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

/// <summary>
/// Endpoint de demonstração/validação: prova que autenticação (JWT) → tenant
/// context (claim familia_id) → RLS (Postgres) funcionam como uma cadeia
/// única. Nenhum familia_id é passado ou filtrado aqui em C# — quem decide
/// quais filhos aparecem é a política de RLS em usuarios_comuns.
/// </summary>
[ApiController]
[Route("api/filhos")]
[Authorize(Roles = MesadaPapeis.Master)]
public sealed class FilhosController(ListarFilhosUseCase listarFilhos) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<FilhoResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var filhos = await listarFilhos.ExecutarAsync(ct);
        var resposta = filhos.Select(f => new FilhoResponse(f.Id, f.Nome, f.Apelido, f.SaldoDevedorAcumulado));
        return Ok(resposta);
    }
}
