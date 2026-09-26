using Mesada.Api.Contracts;
using Mesada.Application.Auth;
using Mesada.Application.Exceptions;
using Mesada.Application.Familias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

/// <summary>Signup (anônimo) e dados da própria família (Master autenticado).</summary>
[ApiController]
[Route("api/familias")]
public sealed class FamiliasController(
    CriarFamiliaUseCase criarFamilia,
    ObterMinhaFamiliaUseCase obterMinhaFamilia,
    AtualizarMinhaFamiliaUseCase atualizarMinhaFamilia) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar([FromBody] CriarFamiliaRequest request, CancellationToken ct)
    {
        try
        {
            var resultado = await criarFamilia.ExecutarAsync(
                request.NomeFamilia, request.NomeMaster, request.EmailMaster, request.SenhaMaster, ct);
            return Created(string.Empty, new TokenResponse(resultado.Token));
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
        catch (EmailJaCadastradoException ex)
        {
            return Conflict(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status409Conflict });
        }
    }

    [HttpGet]
    [Authorize(Roles = MesadaPapeis.Master)]
    [ProducesResponseType<FamiliaResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Obter(CancellationToken ct)
    {
        var familia = await obterMinhaFamilia.ExecutarAsync(ct);
        return Ok(new FamiliaResponse(familia.Id, familia.Nome, familia.CicloFechamentoPadrao));
    }

    [HttpPut]
    [Authorize(Roles = MesadaPapeis.Master)]
    [ProducesResponseType<FamiliaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar([FromBody] AtualizarFamiliaRequest request, CancellationToken ct)
    {
        try
        {
            var familia = await atualizarMinhaFamilia.ExecutarAsync(request.Nome, request.CicloFechamentoPadrao, ct);
            return Ok(new FamiliaResponse(familia.Id, familia.Nome, familia.CicloFechamentoPadrao));
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
    }
}
