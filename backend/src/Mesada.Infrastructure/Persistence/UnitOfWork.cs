using Mesada.Application.Abstractions;

namespace Mesada.Infrastructure.Persistence;

/// <summary>Uma implementação por DbContext concreto — cada caso de uso injeta a que corresponde ao seu contexto (admin ou tenant-scoped).</summary>
public sealed class AdminUnitOfWork(AdminDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}

public sealed class AppUnitOfWork(AppDbContext db) : ITenantUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
