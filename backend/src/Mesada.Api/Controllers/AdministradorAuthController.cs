using Mesada.Api.Contracts;
using Mesada.Application.Administracao;
using Mesada.Application.Auth;
using Mesada.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mesada.Api.Controllers;

/// <summary>Autenticação do painel administrativo interno — nunca acessível por Master/Comum (docs/adr/0004-web-via-lovable.md).</summary>
[ApiController]
[Route("api/admin/auth")]
public sealed class AdministradorAuthController(
    AutenticarAdministradorUseCase autenticar,
    TrocarSenhaAdministradorUseCase trocarSenha,
    AtualizarEmailProprioAdministradorUseCase atualizarEmail) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<TokenAdministradorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginAdministradorRequest request, CancellationToken ct)
    {
        try
        {
            var resultado = await autenticar.ExecutarAsync(request.Email, request.Senha, ct);
            return Ok(new TokenAdministradorResponse(resultado.Token, resultado.DeveTrocarSenha));
        }
        catch (AutenticacaoInvalidaException ex)
        {
            return Unauthorized(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status401Unauthorized });
        }
    }

    [HttpPost("trocar-senha")]
    [Authorize(Roles = MesadaPapeis.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TrocarSenha([FromBody] TrocarSenhaAdministradorRequest request, CancellationToken ct)
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

    [HttpPut("email")]
    [Authorize(Roles = MesadaPapeis.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AtualizarEmail([FromBody] AtualizarEmailAdministradorRequest request, CancellationToken ct)
    {
        try
        {
            await atualizarEmail.ExecutarAsync(request.NovoEmail, ct);
            return NoContent();
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
