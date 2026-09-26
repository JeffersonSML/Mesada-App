using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

/// <summary>Backed por AdminDbContext — Administrador não pertence a nenhuma Familia, não faz sentido em AppDbContext/RLS.</summary>
public interface IAdministradorRepository
{
    /// <summary>Traz o Grupo já carregado (navegação Grupo) — usado no login para montar os claims do JWT.</summary>
    Task<Administrador?> ObterPorEmailComGrupoAsync(string email, CancellationToken ct = default);

    Task<Administrador?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Administrador>> ListarAsync(CancellationToken ct = default);

    void Adicionar(Administrador administrador);
}
