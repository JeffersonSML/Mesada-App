using Mesada.Application.Repositories;
using Mesada.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mesada.Infrastructure.Repositories;

/// <summary>Backed por AdminDbContext — criar uma família (signup) é, por definição, uma operação sem tenant ainda estabelecido.</summary>
public sealed class AdminFamiliaRepository(AdminDbContext db) : IFamiliaRepository
{
    public Task<Domain.Entities.Familia?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Familias.SingleOrDefaultAsync(f => f.Id == id, ct);

    public void Adicionar(Domain.Entities.Familia familia) => db.Familias.Add(familia);
}
