using Mesada.Api.Contracts;
using Mesada.Application.Auth;
using Mesada.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    AutenticarMasterUseCase autenticarMaster,
    ResgatarConviteComumUseCase resgatarConvite,
    ResgatarConviteMasterUseCase resgatarConviteMaster,
    EsqueciSenhaMasterUseCase esqueciSenha,
    RedefinirSenhaMasterUseCase redefinirSenha,
    TrocarSenhaMasterUseCase trocarSenha) : ControllerBase
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

    /// <summary>Resgate de convite por um segundo responsável (Master) — cria a própria conta com e-mail/senha, diferente do resgate Comum (que só vincula um dispositivo).</summary>
    [HttpPost("convites/{codigo}/resgatar-master")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ResgatarConviteMaster(string codigo, [FromBody] ResgatarConviteMasterRequest request, CancellationToken ct)
    {
        try
        {
            var resultado = await resgatarConviteMaster.ExecutarAsync(codigo, request.Email, request.Senha, ct);
            return Ok(new TokenResponse(resultado.Token));
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
        catch (ConviteInvalidoException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
        catch (EmailJaCadastradoException ex)
        {
            return Conflict(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status409Conflict });
        }
    }

    /// <summary>Sempre 204, exista ou não o e-mail — nunca revela se uma conta existe. Se existir, envia o e-mail com o link de redefinição.</summary>
    [HttpPost("master/esqueci-senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> EsqueciSenha([FromBody] EsqueciSenhaMasterRequest request, CancellationToken ct)
    {
        await esqueciSenha.ExecutarAsync(request.Email, ct);
        return NoContent();
    }

    /// <summary>Segunda etapa do "esqueci minha senha" — troca a senha usando o token recebido por e-mail.</summary>
    [HttpPost("master/redefinir-senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaMasterRequest request, CancellationToken ct)
    {
        try
        {
            await redefinirSenha.ExecutarAsync(request.Token, request.NovaSenha, ct);
            return NoContent();
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
    }

    /// <summary>Troca a própria senha (Master já autenticado, informando a senha atual).</summary>
    [HttpPost("master/trocar-senha")]
    [Authorize(Roles = MesadaPapeis.Master)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TrocarSenha([FromBody] TrocarSenhaMasterRequest request, CancellationToken ct)
    {
        try
        {
            await trocarSenha.ExecutarAsync(request.SenhaAtual, request.NovaSenha, ct);
            return NoContent();
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
        catch (AutenticacaoInvalidaException ex)
        {
            return Unauthorized(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status401Unauthorized });
        }
    }
}
