using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

/// <summary>Backed por AppDbContext (mesada_app) — RLS restringe as linhas à própria família, sem filtro em C#.</summary>
public sealed class ConvitesRepository(AppDbContext db) : IConvitesRepository
{
    public async Task<IReadOnlyList<ConviteAcesso>> ListarAsync(CancellationToken ct = default) =>
        await db.ConvitesAcesso
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

    public Task<ConviteAcesso?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.ConvitesAcesso.SingleOrDefaultAsync(c => c.Id == id, ct);

    public void Adicionar(ConviteAcesso convite) => db.ConvitesAcesso.Add(convite);
}
