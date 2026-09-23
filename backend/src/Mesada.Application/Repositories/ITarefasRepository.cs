using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

/// <summary>Tenant-scoped (AppDbContext / mesada_app / RLS).</summary>
public interface ITarefasRepository
{
    Task<IReadOnlyList<Tarefa>> ListarAsync(CancellationToken ct = default);

    Task<Tarefa?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    void Adicionar(Tarefa tarefa);
}
