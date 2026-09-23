using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

/// <summary>Tenant-scoped (AppDbContext / mesada_app / RLS) — TarefaUsuario, a aderência N:N entre Tarefa e UsuarioComum.</summary>
public interface IAderenciasRepository
{
    Task<TarefaUsuario?> ObterAsync(Guid tarefaId, Guid usuarioComumId, CancellationToken ct = default);

    Task<int> ContarAtivasPorUsuarioComumAsync(Guid usuarioComumId, CancellationToken ct = default);

    void Adicionar(TarefaUsuario aderencia);
}
