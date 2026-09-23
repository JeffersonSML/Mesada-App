using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

/// <summary>Tenant-scoped (AppDbContext / mesada_app / RLS) — TarefaUsuario, a aderência N:N entre Tarefa e UsuarioComum.</summary>
public interface IAderenciasRepository
{
    Task<TarefaUsuario?> ObterAsync(Guid tarefaId, Guid usuarioComumId, CancellationToken ct = default);

    /// <summary>Traz a Tarefa relacionada já carregada (navegação Tarefa) — usado pelo motor de cálculo ao marcar uma execução.</summary>
    Task<TarefaUsuario?> ObterPorIdComTarefaAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<TarefaUsuario>> ListarAtivasComTarefaPorUsuarioComumAsync(Guid usuarioComumId, CancellationToken ct = default);

    Task<int> ContarAtivasPorUsuarioComumAsync(Guid usuarioComumId, CancellationToken ct = default);

    void Adicionar(TarefaUsuario aderencia);
}
