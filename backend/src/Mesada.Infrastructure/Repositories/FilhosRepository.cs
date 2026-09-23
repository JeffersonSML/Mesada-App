using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

/// <summary>Backed por AppDbContext (mesada_app) — RLS restringe as linhas à própria família, sem filtro em C#.</summary>
public sealed class FilhosRepository(AppDbContext db) : IFilhosRepository
{
    public Task<UsuarioComum?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.UsuariosComuns.SingleOrDefaultAsync(c => c.Id == id, ct);

    public void Adicionar(UsuarioComum usuarioComum) => db.UsuariosComuns.Add(usuarioComum);
}
