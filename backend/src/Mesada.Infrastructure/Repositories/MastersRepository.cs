using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

/// <summary>Backed por AppDbContext (mesada_app) — RLS restringe as linhas à própria família, sem filtro em C#.</summary>
public sealed class MastersRepository(AppDbContext db) : IMastersRepository
{
    public async Task<IReadOnlyList<UsuarioMaster>> ListarAsync(CancellationToken ct = default) =>
        await db.UsuariosMaster.OrderBy(m => m.Nome).ToListAsync(ct);

    public Task<UsuarioMaster?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.UsuariosMaster.SingleOrDefaultAsync(m => m.Id == id, ct);
}
