using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

/// <summary>Backed por AppDbContext (mesada_app) — RLS já mistura defaults do sistema (familia_id NULL) com as da própria família na leitura.</summary>
public sealed class CategoriasRepository(AppDbContext db) : ICategoriasRepository
{
    public async Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken ct = default) =>
        await db.Categorias
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .ToListAsync(ct);

    public Task<Categoria?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Categorias.SingleOrDefaultAsync(c => c.Id == id, ct);

    public void Adicionar(Categoria categoria) => db.Categorias.Add(categoria);
}
