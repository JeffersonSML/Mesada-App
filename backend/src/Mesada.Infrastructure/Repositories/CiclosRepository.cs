using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Domain.Enums;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

/// <summary>Backed por AppDbContext (mesada_app) — RLS restringe as linhas à própria família, sem filtro em C#.</summary>
public sealed class CiclosRepository(AppDbContext db) : ICiclosRepository
{
    public Task<CicloMesada?> ObterUltimoFechadoAsync(Guid usuarioComumId, CancellationToken ct = default) =>
        db.CiclosMesada
            .Where(c => c.UsuarioComumId == usuarioComumId && c.Status == StatusCiclo.Fechado)
            .OrderByDescending(c => c.DataInicio)
            .FirstOrDefaultAsync(ct);

    public Task<bool> ExisteComDataInicioAsync(Guid usuarioComumId, DateOnly dataInicio, CancellationToken ct = default) =>
        db.CiclosMesada.AnyAsync(c => c.UsuarioComumId == usuarioComumId && c.DataInicio == dataInicio, ct);

    public async Task<IReadOnlyList<CicloMesada>> ListarHistoricoAsync(Guid usuarioComumId, CancellationToken ct = default) =>
        await db.CiclosMesada
            .Where(c => c.UsuarioComumId == usuarioComumId && c.Status == StatusCiclo.Fechado)
            .OrderByDescending(c => c.DataInicio)
            .ToListAsync(ct);

    public void Adicionar(CicloMesada ciclo) => db.CiclosMesada.Add(ciclo);
}
