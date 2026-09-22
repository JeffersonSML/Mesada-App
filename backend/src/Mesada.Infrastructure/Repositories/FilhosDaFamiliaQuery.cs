using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

/// <summary>Backed por AppDbContext (mesada_app) — a consulta abaixo é lida sem filtro de família no C#; quem restringe as linhas é o Postgres via RLS.</summary>
public sealed class FilhosDaFamiliaQuery(AppDbContext db) : IFilhosDaFamiliaQuery
{
    public async Task<IReadOnlyList<UsuarioComum>> ListarAtivosAsync(CancellationToken ct = default) =>
        await db.UsuariosComuns
            .Where(c => c.Status == "ativo")
            .OrderBy(c => c.Nome)
            .ToListAsync(ct);
}
