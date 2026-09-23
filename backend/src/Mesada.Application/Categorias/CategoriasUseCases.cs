using Mesada.Application.Abstractions;
using Mesada.Application.Exceptions;
using Mesada.Application.Repositories;
using Mesada.Domain.Entities;

namespace Mesada.Application.Categorias;

public sealed class ListarCategoriasUseCase(ICategoriasRepository categorias)
{
    public Task<IReadOnlyList<Categoria>> ExecutarAsync(CancellationToken ct = default) =>
        categorias.ListarAsync(ct);
}

/// <summary>Categoria customizada da família — os defaults do sistema (familia_id NULL) só são geridos pelo Administrador (docs/adr/0002-schema-e-rls.md).</summary>
public sealed class CriarCategoriaUseCase(
    ICategoriasRepository categorias,
    ITenantContextAccessor tenantContext,
    ITenantUnitOfWork unitOfWork)
{
    public async Task<Categoria> ExecutarAsync(string nome, Guid? parentId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("Nome da categoria é obrigatório.");

        if (parentId is not null && await categorias.ObterPorIdAsync(parentId.Value, ct) is null)
            throw new RecursoNaoEncontradoException("Categoria pai não encontrada.");

        var categoria = new Categoria
        {
            Id = Guid.NewGuid(),
            FamiliaId = tenantContext.FamiliaId ?? throw new InvalidOperationException("Requisição sem contexto de família."),
            ParentId = parentId,
            Nome = nome,
            Sistema = false,
            Ativo = true,
        };

        categorias.Adicionar(categoria);
        await unitOfWork.SaveChangesAsync(ct);
        return categoria;
    }
}

public sealed class AtualizarCategoriaUseCase(
    ICategoriasRepository categorias,
    ITenantUnitOfWork unitOfWork)
{
    public async Task<Categoria> ExecutarAsync(Guid id, string nome, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("Nome da categoria é obrigatório.");

        var categoria = await categorias.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Categoria não encontrada.");

        if (categoria.Sistema)
            throw new ValidacaoException("Categorias padrão do sistema não podem ser editadas pela família.");

        categoria.Nome = nome;
        await unitOfWork.SaveChangesAsync(ct);
        return categoria;
    }
}

/// <summary>Exclusão lógica (ativo = false) — categorias podem estar referenciadas por tarefas, então nunca são removidas fisicamente.</summary>
public sealed class RemoverCategoriaUseCase(
    ICategoriasRepository categorias,
    ITenantUnitOfWork unitOfWork)
{
    public async Task ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        var categoria = await categorias.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Categoria não encontrada.");

        if (categoria.Sistema)
            throw new ValidacaoException("Categorias padrão do sistema não podem ser removidas pela família.");

        categoria.Ativo = false;
        await unitOfWork.SaveChangesAsync(ct);
    }
}
