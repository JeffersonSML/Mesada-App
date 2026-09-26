using System.Security.Cryptography;
using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Entities;

namespace Mesada.Application.Administracao;

public sealed record ResultadoConviteAdministrador(Administrador Administrador, string SenhaTemporaria);

public sealed class ListarAdministradoresUseCase(IAdministradorRepository administradores)
{
    public Task<IReadOnlyList<Administrador>> ExecutarAsync(CancellationToken ct = default) =>
        administradores.ListarAsync(ct);
}

/// <summary>
/// O Owner convida uma pessoa da equipe (ex.: "Tecnologia") para o painel
/// administrativo — gera uma senha temporária de uso único, devolvida só
/// nesta chamada; o convidado é obrigado a trocá-la no primeiro login
/// (DeveTrocarSenha=true).
/// </summary>
public sealed class ConvidarAdministradorUseCase(
    IAdministradorRepository administradores,
    IGrupoAdministradorRepository grupos,
    IPasswordHasher hasher,
    IUnitOfWork unitOfWork,
    ITenantContextAccessor tenantContext)
{
    private const string AlfabetoSenha = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
    private const int TamanhoSenha = 16;

    public async Task<ResultadoConviteAdministrador> ExecutarAsync(string nome, string email, Guid grupoId, CancellationToken ct = default)
    {
        ExigirOwner.Validar(tenantContext);

        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("Nome é obrigatório.");
        if (await administradores.ObterPorEmailComGrupoAsync(email, ct) is not null)
            throw new EmailJaCadastradoException(email);

        var grupo = await grupos.ObterPorIdAsync(grupoId, ct)
            ?? throw new RecursoNaoEncontradoException("Grupo não encontrado.");

        var senhaTemporaria = new string(RandomNumberGenerator.GetItems<char>(AlfabetoSenha, TamanhoSenha));
        var administrador = new Administrador
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Email = email,
            SenhaHash = hasher.Hash(senhaTemporaria),
            GrupoId = grupo.Id,
            Grupo = grupo,
            DeveTrocarSenha = true,
            Status = "ativo",
        };

        administradores.Adicionar(administrador);
        await unitOfWork.SaveChangesAsync(ct);
        return new ResultadoConviteAdministrador(administrador, senhaTemporaria);
    }
}

public sealed class AtualizarAdministradorUseCase(
    IAdministradorRepository administradores,
    IGrupoAdministradorRepository grupos,
    IUnitOfWork unitOfWork,
    ITenantContextAccessor tenantContext)
{
    public async Task<Administrador> ExecutarAsync(Guid id, string nome, Guid grupoId, CancellationToken ct = default)
    {
        ExigirOwner.Validar(tenantContext);

        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("Nome é obrigatório.");

        var administrador = await administradores.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Administrador não encontrado.");
        var grupo = await grupos.ObterPorIdAsync(grupoId, ct)
            ?? throw new RecursoNaoEncontradoException("Grupo não encontrado.");

        administrador.Nome = nome;
        administrador.GrupoId = grupo.Id;
        administrador.Grupo = grupo;

        await unitOfWork.SaveChangesAsync(ct);
        return administrador;
    }
}

/// <summary>
/// Desativação lógica (nunca remoção física — auditoria). Bloqueia
/// autodesativação e desativar o último Owner ativo, para nunca deixar o
/// sistema sem ninguém capaz de gerenciar administradores.
/// </summary>
public sealed class DesativarAdministradorUseCase(
    IAdministradorRepository administradores,
    IGrupoAdministradorRepository grupos,
    IUnitOfWork unitOfWork,
    ITenantContextAccessor tenantContext)
{
    public async Task ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        ExigirOwner.Validar(tenantContext);

        if (id == tenantContext.AdministradorId)
            throw new ValidacaoException("Você não pode desativar a própria conta.");

        var administrador = await administradores.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Administrador não encontrado.");

        if (administrador.GrupoId is { } grupoId)
        {
            var grupo = await grupos.ObterPorIdAsync(grupoId, ct);
            if (grupo is { Sistema: true })
            {
                var todos = await administradores.ListarAsync(ct);
                var ownersAtivos = todos.Count(a => a.GrupoId == grupoId && a.Status == "ativo");
                if (ownersAtivos <= 1)
                    throw new ValidacaoException("Não é possível desativar o último administrador Owner ativo.");
            }
        }

        administrador.Status = "inativo";
        await unitOfWork.SaveChangesAsync(ct);
    }
}
