using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

/// <summary>Backed por AppDbContext (mesada_app) — RLS já restringe a uma única linha (a própria família), sem filtro em C#.</summary>
public sealed class MinhaFamiliaRepository(AppDbContext db) : IMinhaFamiliaRepository
{
    public Task<Familia?> ObterAsync(CancellationToken ct = default) =>
        db.Familias.SingleOrDefaultAsync(ct);
}
