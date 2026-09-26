using System.Text.Json;
using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Entities;

namespace Mesada.Application.Administracao;

/// <summary>Ações restritas ao grupo Owner (acesso total) do painel administrativo — só ele gerencia quem tem acesso ao sistema e o que cada grupo pode fazer.</summary>
internal static class ExigirOwner
{
    public static void Validar(ITenantContextAccessor tenantContext)
    {
        if (!tenantContext.AdministradorEhOwner)
            throw new PermissaoNegadaException("Esta ação é restrita ao grupo Owner do painel administrativo.");
    }
}

internal static class PermissoesJson
{
    public static string Serializar(IReadOnlyDictionary<string, bool>? permissoes) =>
        JsonSerializer.Serialize(permissoes ?? new Dictionary<string, bool>());

    public static IReadOnlyDictionary<string, bool> Desserializar(string permissoesJson) =>
        JsonSerializer.Deserialize<Dictionary<string, bool>>(permissoesJson) ?? new Dictionary<string, bool>();
}

public sealed class ListarGruposAdministradorUseCase(IGrupoAdministradorRepository grupos)
{
    public Task<IReadOnlyList<GrupoAdministrador>> ExecutarAsync(CancellationToken ct = default) =>
        grupos.ListarAsync(ct);
}

public sealed class CriarGrupoAdministradorUseCase(
    IGrupoAdministradorRepository grupos,
    IUnitOfWork unitOfWork,
    ITenantContextAccessor tenantContext)
{
    public async Task<GrupoAdministrador> ExecutarAsync(
        string nome, string? descricao, IReadOnlyDictionary<string, bool>? permissoes, CancellationToken ct = default)
    {
        ExigirOwner.Validar(tenantContext);

        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("Nome do grupo é obrigatório.");
        if (await grupos.ObterPorNomeAsync(nome, ct) is not null)
            throw new ValidacaoException("Já existe um grupo com este nome.");

        var grupo = new GrupoAdministrador
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Descricao = descricao,
            Permissoes = PermissoesJson.Serializar(permissoes),
            Sistema = false,
        };

        grupos.Adicionar(grupo);
        await unitOfWork.SaveChangesAsync(ct);
        return grupo;
    }
}

public sealed class AtualizarGrupoAdministradorUseCase(
    IGrupoAdministradorRepository grupos,
    IUnitOfWork unitOfWork,
    ITenantContextAccessor tenantContext)
{
    public async Task<GrupoAdministrador> ExecutarAsync(
        Guid id, string nome, string? descricao, IReadOnlyDictionary<string, bool>? permissoes, CancellationToken ct = default)
    {
        ExigirOwner.Validar(tenantContext);

        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("Nome do grupo é obrigatório.");

        var grupo = await grupos.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Grupo não encontrado.");

        if (grupo.Sistema)
            throw new ValidacaoException("O grupo Owner não pode ser editado.");

        grupo.Nome = nome;
        grupo.Descricao = descricao;
        grupo.Permissoes = PermissoesJson.Serializar(permissoes);

        await unitOfWork.SaveChangesAsync(ct);
        return grupo;
    }
}

public sealed class RemoverGrupoAdministradorUseCase(
    IGrupoAdministradorRepository grupos,
    IUnitOfWork unitOfWork,
    ITenantContextAccessor tenantContext)
{
    public async Task ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        ExigirOwner.Validar(tenantContext);

        var grupo = await grupos.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Grupo não encontrado.");

        if (grupo.Sistema)
            throw new ValidacaoException("O grupo Owner não pode ser removido.");
        if (await grupos.TemAdministradoresVinculadosAsync(id, ct))
            throw new ValidacaoException("Não é possível remover um grupo com administradores vinculados — mova-os para outro grupo primeiro.");

        grupos.Remover(grupo);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
