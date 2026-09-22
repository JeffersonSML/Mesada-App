using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;

namespace Mesada.Application.Auth;

public sealed record ResultadoAutenticacaoMaster(string Token, Guid FamiliaId, Guid UsuarioMasterId, bool IsFinanceiro);

/// <summary>Login de UsuarioMaster (pai/mãe/responsável) por e-mail e senha.</summary>
public sealed class AutenticarMasterUseCase(
    IUsuarioMasterRepository masters,
    IPasswordHasher hasher,
    IJwtTokenService tokens)
{
    public async Task<ResultadoAutenticacaoMaster> ExecutarAsync(string email, string senha, CancellationToken ct = default)
    {
        var usuario = await masters.ObterPorEmailAsync(email, ct);
        if (usuario is null || usuario.Status != "ativo" || !hasher.Verificar(senha, usuario.SenhaHash))
            throw new AutenticacaoInvalidaException();

        var token = tokens.GerarTokenMaster(usuario);
        return new ResultadoAutenticacaoMaster(token, usuario.FamiliaId, usuario.Id, usuario.IsFinanceiro);
    }
}
