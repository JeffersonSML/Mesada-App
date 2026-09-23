using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Domain.Enums;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

/// <summary>Backed por AppDbContext (mesada_app) — RLS restringe as linhas à própria família, sem filtro em C#.</summary>
public sealed class ExecucoesRepository(AppDbContext db) : IExecucoesRepository
{
    public Task<Execucao?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Execucoes.SingleOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<Execucao>> ListarPendentesAprovacaoAsync(CancellationToken ct = default) =>
        await db.Execucoes
            .Include(e => e.TarefaUsuario)
            .ThenInclude(tu => tu!.Tarefa)
            .Where(e => e.StatusAprovacao == StatusAprovacao.Pendente)
            .OrderBy(e => e.DataExecucao)
            .ToListAsync(ct);

    public void Adicionar(Execucao execucao) => db.Execucoes.Add(execucao);
}
