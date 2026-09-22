using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

/// <summary>Backed por AdminDbContext — código de convite é UNIQUE globalmente, então a busca por código não vaza dado entre famílias (ver AdminUsuarioMasterRepository).</summary>
public sealed class AdminConviteAcessoRepository(AdminDbContext db) : IConviteAcessoRepository
{
    public Task<ConviteAcesso?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default) =>
        db.ConvitesAcesso.SingleOrDefaultAsync(c => c.Codigo == codigo, ct);

    public void Adicionar(ConviteAcesso convite) => db.ConvitesAcesso.Add(convite);
}
