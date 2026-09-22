using Mesada.Api.Contracts;
using Mesada.Application.Auth;
using Mesada.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    AutenticarMasterUseCase autenticarMaster,
    ResgatarConviteComumUseCase resgatarConvite) : ControllerBase
{
    /// <summary>Login do Master (pai/mãe/responsável) por e-mail e senha — cadastro exclusivo da Web.</summary>
    [HttpPost("master/login")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginMaster([FromBody] LoginMasterRequest request, CancellationToken ct)
    {
        try
        {
            var resultado = await autenticarMaster.ExecutarAsync(request.Email, request.Senha, ct);
            return Ok(new TokenResponse(resultado.Token));
        }
        catch (AutenticacaoInvalidaException ex)
        {
            return Unauthorized(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status401Unauthorized });
        }
    }

    /// <summary>
    /// Resgate de convite pelo app mobile do filho (usuário Comum): código
    /// gerado pelo Master na Web, vincula o dispositivo e emite o JWT do
    /// filho (docs/especificacao.md#fluxo-de-convite).
    /// </summary>
    [HttpPost("convites/{codigo}/resgatar")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResgatarConvite(string codigo, [FromBody] ResgatarConviteRequest request, CancellationToken ct)
    {
        try
        {
            var resultado = await resgatarConvite.ExecutarAsync(codigo, request.DispositivoId, ct);
            return Ok(new TokenResponse(resultado.Token));
        }
        catch (ConviteInvalidoException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
    }
}
