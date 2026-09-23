using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

/// <summary>Backed por AppDbContext (mesada_app) — RLS restringe as linhas à própria família, sem filtro em C#.</summary>
public sealed class AderenciasRepository(AppDbContext db) : IAderenciasRepository
{
    public Task<TarefaUsuario?> ObterAsync(Guid tarefaId, Guid usuarioComumId, CancellationToken ct = default) =>
        db.TarefasUsuarios.SingleOrDefaultAsync(a => a.TarefaId == tarefaId && a.UsuarioComumId == usuarioComumId, ct);

    public Task<int> ContarAtivasPorUsuarioComumAsync(Guid usuarioComumId, CancellationToken ct = default) =>
        db.TarefasUsuarios.CountAsync(a => a.UsuarioComumId == usuarioComumId && a.Ativo, ct);

    public void Adicionar(TarefaUsuario aderencia) => db.TarefasUsuarios.Add(aderencia);
}
