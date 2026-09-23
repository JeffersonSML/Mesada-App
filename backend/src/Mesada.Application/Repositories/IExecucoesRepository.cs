using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

/// <summary>Tenant-scoped (AppDbContext / mesada_app / RLS).</summary>
public interface IExecucoesRepository
{
    Task<Execucao?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Execucao>> ListarPendentesAprovacaoAsync(CancellationToken ct = default);

    /// <summary>
    /// Execuções ainda não consolidadas em nenhum ciclo (ciclo_mesada_id
    /// IS NULL) de um filho, dentro de um período — só as já resolvidas
    /// (Aprovado ou NaoAplicavel; Pendente/Rejeitado nunca entram no
    /// fechamento). Traz Tarefa/TarefaUsuario já carregados.
    /// </summary>
    Task<IReadOnlyList<Execucao>> ListarNaoConsolidadasAsync(
        Guid usuarioComumId, DateOnly dataInicio, DateOnly dataFim, CancellationToken ct = default);

    void Adicionar(Execucao execucao);
}
