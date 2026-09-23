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

    public async Task<IReadOnlyList<Execucao>> ListarNaoConsolidadasAsync(
        Guid usuarioComumId, DateOnly dataInicio, DateOnly dataFim, CancellationToken ct = default)
    {
        var inicio = dataInicio.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var fimExclusivo = dataFim.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        return await db.Execucoes
            .Include(e => e.TarefaUsuario)
            .ThenInclude(tu => tu!.Tarefa)
            .Where(e => e.CicloMesadaId == null
                && e.TarefaUsuario!.UsuarioComumId == usuarioComumId
                && e.DataExecucao >= inicio && e.DataExecucao < fimExclusivo
                && (e.StatusAprovacao == StatusAprovacao.Aprovado || e.StatusAprovacao == StatusAprovacao.NaoAplicavel))
            .ToListAsync(ct);
    }

    public void Adicionar(Execucao execucao) => db.Execucoes.Add(execucao);
}
