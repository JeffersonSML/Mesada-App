using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

/// <summary>Tenant-scoped (AppDbContext / mesada_app / RLS).</summary>
public interface ICiclosRepository
{
    Task<CicloMesada?> ObterUltimoFechadoAsync(Guid usuarioComumId, CancellationToken ct = default);

    Task<bool> ExisteComDataInicioAsync(Guid usuarioComumId, DateOnly dataInicio, CancellationToken ct = default);

    Task<IReadOnlyList<CicloMesada>> ListarHistoricoAsync(Guid usuarioComumId, CancellationToken ct = default);

    void Adicionar(CicloMesada ciclo);
}
