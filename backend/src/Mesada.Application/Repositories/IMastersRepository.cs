using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

/// <summary>Tenant-scoped (AppDbContext / mesada_app / RLS) — distinto de IUsuarioMasterRepository (admin-backed, usado só no login/signup pré-tenant).</summary>
public interface IMastersRepository
{
    Task<IReadOnlyList<UsuarioMaster>> ListarAsync(CancellationToken ct = default);

    Task<UsuarioMaster?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
}
