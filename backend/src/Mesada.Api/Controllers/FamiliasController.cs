using Mesada.Api.Contracts;
using Mesada.Application.Exceptions;
using Mesada.Application.Familias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

/// <summary>Signup — cadastro de uma nova família (cadastro exclusivo da Web).</summary>
[ApiController]
[Route("api/familias")]
public sealed class FamiliasController(CriarFamiliaUseCase criarFamilia) : ControllerBase
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
}
