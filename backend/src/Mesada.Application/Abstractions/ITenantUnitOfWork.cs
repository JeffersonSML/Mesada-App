namespace Mesada.Application.Abstractions;

/// <summary>
/// Idêntico em forma a IUnitOfWork, mas um tipo distinto de propósito: uma
/// interface separada para o AppDbContext (mesada_app, RLS) evita colisão
/// de registro no DI com IUnitOfWork, que já é usado pelo AdminDbContext
/// (mesada_admin) nos fluxos pré-tenant (login, resgate de convite — ver
/// docs/adr/0003-autenticacao-e-tenant-context.md). Casos de uso
/// autenticados e tenant-scoped (Master/Comum operando dentro da própria
/// família) usam este; os pré-tenant continuam usando IUnitOfWork.
/// </summary>
public interface ITenantUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
