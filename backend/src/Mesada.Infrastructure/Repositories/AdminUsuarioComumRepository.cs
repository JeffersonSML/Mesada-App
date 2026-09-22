using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

/// <summary>Backed por AdminDbContext — usado no resgate de convite, quando ainda não há contexto de família estabelecido (ver AdminUsuarioMasterRepository).</summary>
public sealed class AdminUsuarioComumRepository(AdminDbContext db) : IUsuarioComumRepository
{
    public Task<UsuarioComum?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.UsuariosComuns.SingleOrDefaultAsync(c => c.Id == id, ct);

    public void Adicionar(UsuarioComum usuarioComum) => db.UsuariosComuns.Add(usuarioComum);
}
