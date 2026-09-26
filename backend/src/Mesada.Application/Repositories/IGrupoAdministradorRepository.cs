using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

/// <summary>Backed por AdminDbContext.</summary>
public interface IGrupoAdministradorRepository
{
    Task<GrupoAdministrador?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    Task<GrupoAdministrador?> ObterPorNomeAsync(string nome, CancellationToken ct = default);

    Task<IReadOnlyList<GrupoAdministrador>> ListarAsync(CancellationToken ct = default);

    Task<bool> TemAdministradoresVinculadosAsync(Guid grupoId, CancellationToken ct = default);

    void Adicionar(GrupoAdministrador grupo);

    void Remover(GrupoAdministrador grupo);
}
