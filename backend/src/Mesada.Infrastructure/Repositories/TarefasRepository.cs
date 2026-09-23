using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

/// <summary>Backed por AppDbContext (mesada_app) — RLS restringe as linhas à própria família, sem filtro em C#.</summary>
public sealed class TarefasRepository(AppDbContext db) : ITarefasRepository
{
    public async Task<IReadOnlyList<Tarefa>> ListarAsync(CancellationToken ct = default) =>
        await db.Tarefas
            .Where(t => t.Ativo)
            .OrderBy(t => t.Nome)
            .ToListAsync(ct);

    public Task<Tarefa?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Tarefas.SingleOrDefaultAsync(t => t.Id == id, ct);

    public void Adicionar(Tarefa tarefa) => db.Tarefas.Add(tarefa);
}
