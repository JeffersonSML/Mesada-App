using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

/// <summary>
/// Tenant-scoped (AppDbContext / mesada_app / RLS) — a política de SELECT
/// (infra/db/migrations/019_rls_policies.sql) já retorna os defaults do
/// sistema (familia_id NULL) somados às categorias da própria família, sem
/// nenhum filtro em C#.
/// </summary>
public interface ICategoriasRepository
{
    Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken ct = default);

    Task<Categoria?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    void Adicionar(Categoria categoria);
}
