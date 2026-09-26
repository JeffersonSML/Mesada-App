using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;

namespace Mesada.Application.Administracao;

public sealed record ResultadoAutenticacaoAdministrador(string Token, Guid AdministradorId, bool DeveTrocarSenha);

/// <summary>
/// Login do painel administrativo interno — nunca acessível por Master/Comum.
/// Backed por AdminDbContext (mesada_admin), fora de qualquer contexto de
/// família (ver docs/adr/0004-web-via-lovable.md e o Módulo Administrador).
/// </summary>
public sealed class AutenticarAdministradorUseCase(
    IAdministradorRepository administradores,
    IPasswordHasher hasher,
    IJwtTokenService tokens)
{
    public async Task<ResultadoAutenticacaoAdministrador> ExecutarAsync(string email, string senha, CancellationToken ct = default)
    {
        var administrador = await administradores.ObterPorEmailComGrupoAsync(email, ct);
        if (administrador is null || administrador.Status != "ativo" || !hasher.Verificar(senha, administrador.SenhaHash))
            throw new AutenticacaoInvalidaException();

        var token = tokens.GerarTokenAdministrador(administrador);
        return new ResultadoAutenticacaoAdministrador(token, administrador.Id, administrador.DeveTrocarSenha);
    }
}

/// <summary>Troca a própria senha — obrigatória logo após criação/reset (DeveTrocarSenha=true), mas disponível a qualquer momento.</summary>
public sealed class TrocarSenhaAdministradorUseCase(
    IAdministradorRepository administradores,
    IPasswordHasher hasher,
    IUnitOfWork unitOfWork,
    ITenantContextAccessor tenantContext)
{
    private const int TamanhoMinimoSenha = 8;

    public async Task ExecutarAsync(string senhaAtual, string novaSenha, CancellationToken ct = default)
    {
        if (novaSenha.Length < TamanhoMinimoSenha)
            throw new ValidacaoException($"Nova senha deve ter ao menos {TamanhoMinimoSenha} caracteres.");

        var administradorId = tenantContext.AdministradorId
            ?? throw new InvalidOperationException("Requisição sem administrador autenticado.");
        var administrador = await administradores.ObterPorIdAsync(administradorId, ct)
            ?? throw new InvalidOperationException("Administrador autenticado não existe mais.");

        if (!hasher.Verificar(senhaAtual, administrador.SenhaHash))
            throw new AutenticacaoInvalidaException();

        administrador.SenhaHash = hasher.Hash(novaSenha);
        administrador.DeveTrocarSenha = false;
        await unitOfWork.SaveChangesAsync(ct);
    }
}

/// <summary>Permite ao próprio Administrador trocar seu e-mail de login — pedido explícito do Owner (poder alterar o e-mail cadastrado no futuro).</summary>
public sealed class AtualizarEmailProprioAdministradorUseCase(
    IAdministradorRepository administradores,
    IUnitOfWork unitOfWork,
    ITenantContextAccessor tenantContext)
{
    public async Task ExecutarAsync(string novoEmail, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(novoEmail))
            throw new ValidacaoException("E-mail é obrigatório.");

        var administradorId = tenantContext.AdministradorId
            ?? throw new InvalidOperationException("Requisição sem administrador autenticado.");

        var existente = await administradores.ObterPorEmailComGrupoAsync(novoEmail, ct);
        if (existente is not null && existente.Id != administradorId)
            throw new EmailJaCadastradoException(novoEmail);

        var administrador = await administradores.ObterPorIdAsync(administradorId, ct)
            ?? throw new InvalidOperationException("Administrador autenticado não existe mais.");

        administrador.Email = novoEmail;
        await unitOfWork.SaveChangesAsync(ct);
    }
}
