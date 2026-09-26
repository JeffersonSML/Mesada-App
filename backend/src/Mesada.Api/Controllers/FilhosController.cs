using Mesada.Api.Contracts;
using Mesada.Application.Auth;
using Mesada.Application.Exceptions;
using Mesada.Application.Familias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

/// <summary>
/// Cadastro de filhos (UsuarioComum) — cadastro exclusivo da Web, restrito
/// ao Master. A leitura (GET) prova que autenticação (JWT) → tenant context
/// (claim familia_id) → RLS (Postgres) funcionam como uma cadeia única:
/// nenhum familia_id é passado ou filtrado aqui em C#, é a política de RLS
/// em usuarios_comuns quem decide quais filhos aparecem.
/// </summary>
[ApiController]
[Route("api/filhos")]
[Authorize(Roles = MesadaPapeis.Master)]
public sealed class FilhosController(
    ListarFilhosUseCase listarFilhos,
    CriarFilhoUseCase criarFilho,
    AtualizarFilhoUseCase atualizarFilho,
    RemoverFilhoUseCase removerFilho) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<FilhoResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var filhos = await listarFilhos.ExecutarAsync(ct);
        var resposta = filhos.Select(f => new FilhoResponse(f.Id, f.Nome, f.Apelido, f.SaldoDevedorAcumulado));
        return Ok(resposta);
    }

    [HttpPost]
    [ProducesResponseType<FilhoDetalheResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarFilhoRequest request, CancellationToken ct)
    {
        try
        {
            var filho = await criarFilho.ExecutarAsync(
                request.Nome, request.Apelido, request.CicloFechamento, request.MesadaBase, request.ValorPonto, ct);
            var resposta = ParaDetalhe(filho);
            return CreatedAtAction(nameof(Listar), null, resposta);
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<FilhoDetalheResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarFilhoRequest request, CancellationToken ct)
    {
        try
        {
            var filho = await atualizarFilho.ExecutarAsync(
                id, request.Nome, request.Apelido, request.CicloFechamento, request.MesadaBase, request.ValorPonto, ct);
            return Ok(ParaDetalhe(filho));
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
            await removerFilho.ExecutarAsync(id, ct);
            return NoContent();
        }
        catch (RecursoNaoEncontradoException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status404NotFound });
        }
    }

    private static FilhoDetalheResponse ParaDetalhe(Mesada.Domain.Entities.UsuarioComum filho) => new(
        filho.Id, filho.Nome, filho.Apelido, filho.CicloFechamento, filho.MesadaBase, filho.ValorPonto, filho.SaldoDevedorAcumulado);
}
