using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

public sealed class AdminGrupoAdministradorRepository(AdminDbContext db) : IGrupoAdministradorRepository
{
    public Task<GrupoAdministrador?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.GruposAdministrador.SingleOrDefaultAsync(g => g.Id == id, ct);

    public Task<GrupoAdministrador?> ObterPorNomeAsync(string nome, CancellationToken ct = default) =>
        db.GruposAdministrador.SingleOrDefaultAsync(g => g.Nome == nome, ct);

    public async Task<IReadOnlyList<GrupoAdministrador>> ListarAsync(CancellationToken ct = default) =>
        await db.GruposAdministrador.OrderBy(g => g.Nome).ToListAsync(ct);

    public Task<bool> TemAdministradoresVinculadosAsync(Guid grupoId, CancellationToken ct = default) =>
        db.Administradores.AnyAsync(a => a.GrupoId == grupoId, ct);

    public void Adicionar(GrupoAdministrador grupo) => db.GruposAdministrador.Add(grupo);

    public void Remover(GrupoAdministrador grupo) => db.GruposAdministrador.Remove(grupo);
}
