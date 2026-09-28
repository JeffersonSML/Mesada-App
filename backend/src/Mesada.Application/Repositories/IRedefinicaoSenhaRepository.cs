using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

/// <summary>Admin-backed (AdminDbContext / mesada_admin) — fluxo sempre pré-tenant, igual IUsuarioMasterRepository.</summary>
public interface IRedefinicaoSenhaRepository
{
    Task<RedefinicaoSenhaMaster?> ObterPorTokenHashAsync(string tokenHash, CancellationToken ct = default);

    void Adicionar(RedefinicaoSenhaMaster redefinicao);
}
