using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

/// <summary>
/// Backed por AdminDbContext (mesada_admin, BYPASSRLS): login por e-mail
/// precisa localizar o usuário ANTES de sabermos a que família ele
/// pertence, então não há app.current_familia_id para escopar a consulta
/// via mesada_app. Seguro porque email é UNIQUE globalmente — a busca não
/// pode vazar dado de outra família, só identifica o próprio tenant do
/// usuário autenticando.
/// </summary>
public sealed class AdminUsuarioMasterRepository(AdminDbContext db) : IUsuarioMasterRepository
{
    public Task<UsuarioMaster?> ObterPorEmailAsync(string email, CancellationToken ct = default) =>
        db.UsuariosMaster.SingleOrDefaultAsync(m => m.Email == email, ct);

    public Task<UsuarioMaster?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.UsuariosMaster.SingleOrDefaultAsync(m => m.Id == id, ct);

    public Task<bool> ExisteMasterFinanceiroAtivoAsync(Guid familiaId, CancellationToken ct = default) =>
        db.UsuariosMaster.AnyAsync(m => m.FamiliaId == familiaId && m.IsFinanceiro && m.Status == "ativo", ct);

    public void Adicionar(UsuarioMaster usuarioMaster) => db.UsuariosMaster.Add(usuarioMaster);
}
