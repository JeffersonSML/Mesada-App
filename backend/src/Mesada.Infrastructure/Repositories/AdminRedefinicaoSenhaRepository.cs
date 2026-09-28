using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

/// <summary>
/// Backed por AdminDbContext (mesada_admin, BYPASSRLS) — o pedido e o uso
/// do link de redefinição sempre acontecem antes do login, sem
/// app.current_familia_id disponível (mesmo raciocínio de
/// AdminUsuarioMasterRepository).
/// </summary>
public sealed class AdminRedefinicaoSenhaRepository(AdminDbContext db) : IRedefinicaoSenhaRepository
{
    public Task<RedefinicaoSenhaMaster?> ObterPorTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        db.RedefinicoesSenhaMaster.SingleOrDefaultAsync(r => r.TokenHash == tokenHash, ct);

    public void Adicionar(RedefinicaoSenhaMaster redefinicao) => db.RedefinicoesSenhaMaster.Add(redefinicao);
}
