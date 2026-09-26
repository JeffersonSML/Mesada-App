using Mesada.Application.Repositories;
using Mesada.Domain.Entities;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

public sealed class AdminAdministradorRepository(AdminDbContext db) : IAdministradorRepository
{
    public Task<Administrador?> ObterPorEmailComGrupoAsync(string email, CancellationToken ct = default) =>
        db.Administradores.Include(a => a.Grupo).SingleOrDefaultAsync(a => a.Email == email, ct);

    public Task<Administrador?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Administradores.SingleOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Administrador>> ListarAsync(CancellationToken ct = default) =>
        await db.Administradores.Include(a => a.Grupo).OrderBy(a => a.Nome).ToListAsync(ct);

    public void Adicionar(Administrador administrador) => db.Administradores.Add(administrador);
}
