using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

/// <summary>Tenant-scoped (AppDbContext / mesada_app / RLS).</summary>
public interface IExecucoesRepository
{
    Task<Execucao?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Execucao>> ListarPendentesAprovacaoAsync(CancellationToken ct = default);

    void Adicionar(Execucao execucao);
}
