using Mesada.Domain.Entities;

namespace Mesada.Application.Repositories;

public interface IUsuarioMasterRepository
{
    Task<UsuarioMaster?> ObterPorEmailAsync(string email, CancellationToken ct = default);

    Task<UsuarioMaster?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExisteMasterFinanceiroAtivoAsync(Guid familiaId, CancellationToken ct = default);

    void Adicionar(UsuarioMaster usuarioMaster);
}
